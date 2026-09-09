using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SkiaSharp;
using UserManagement.Mobile.Core.Services.Interfaces;

namespace UserManagement.Mobile.Core.Services.Implementations;

/// <summary>
/// On-device face recognition using ONNX Runtime with ArcFace model.
/// Generates 512-dim face embeddings and performs cosine-similarity matching.
/// </summary>
public sealed class FaceRecognitionService : IFaceRecognitionService, IDisposable
{
    private const int ImageSize = 112; // ArcFace input size
    private const int EmbeddingDimension = 512;

    private InferenceSession? _session;
    private readonly SemaphoreSlim _loadLock = new(1, 1);

    /// <summary>Function to resolve the ONNX model file path. Set by the platform host.</summary>
    public Func<Task<string>>? ModelPathResolver { get; set; }

    public bool IsModelLoaded => _session is not null;

    public async Task LoadModelAsync()
    {
        if (_session is not null) return;

        await _loadLock.WaitAsync();
        try
        {
            if (_session is not null) return;

            var modelPath = ModelPathResolver is not null
                ? await ModelPathResolver()
                : throw new InvalidOperationException("ModelPathResolver must be set before loading the model.");

            if (!File.Exists(modelPath))
                throw new FileNotFoundException("ArcFace ONNX model not found.", modelPath);

            var options = new SessionOptions();
            options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;

            _session = new InferenceSession(modelPath, options);
        }
        finally
        {
            _loadLock.Release();
        }
    }

    public void UnloadModel()
    {
        _session?.Dispose();
        _session = null;
    }

    public Task<float[]> GenerateEmbeddingAsync(byte[] faceImageData)
    {
        if (_session is null)
            throw new InvalidOperationException("Model not loaded. Call LoadModelAsync first.");

        return Task.Run(() =>
        {
            // Decode and preprocess the face image
            var inputTensor = PreprocessImage(faceImageData);

            // Run inference
            var inputName = _session.InputMetadata.Keys.First();
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(inputName, inputTensor)
            };

            using var results = _session.Run(inputs);
            var outputTensor = results.First().AsTensor<float>();

            // Extract and L2-normalize the embedding
            var embedding = new float[EmbeddingDimension];
            for (int i = 0; i < EmbeddingDimension; i++)
            {
                embedding[i] = outputTensor[0, i];
            }

            L2Normalize(embedding);
            return embedding;
        });
    }

    public FaceMatchResult Verify(float[] capturedEmbedding, float[] enrolledEmbedding, float threshold = 0.55f)
    {
        if (capturedEmbedding.Length != EmbeddingDimension || enrolledEmbedding.Length != EmbeddingDimension)
        {
            return new FaceMatchResult
            {
                IsMatch = false,
                Confidence = 0f,
                FailureReason = $"Invalid embedding dimension. Expected {EmbeddingDimension}."
            };
        }

        var similarity = CosineSimilarity(capturedEmbedding, enrolledEmbedding);

        return new FaceMatchResult
        {
            IsMatch = similarity >= threshold,
            Confidence = Math.Clamp(similarity, 0f, 1f),
            FailureReason = similarity < threshold ? "Face did not match. Please try again." : null
        };
    }

    public float[] AverageEmbeddings(IReadOnlyList<float[]> embeddings)
    {
        if (embeddings.Count == 0)
            throw new ArgumentException("At least one embedding is required.", nameof(embeddings));

        var averaged = new float[EmbeddingDimension];

        foreach (var emb in embeddings)
        {
            for (int i = 0; i < EmbeddingDimension; i++)
            {
                averaged[i] += emb[i];
            }
        }

        var count = (float)embeddings.Count;
        for (int i = 0; i < EmbeddingDimension; i++)
        {
            averaged[i] /= count;
        }

        L2Normalize(averaged);
        return averaged;
    }

    public byte[] EmbeddingToBytes(float[] embedding)
    {
        var bytes = new byte[embedding.Length * sizeof(float)];
        Buffer.BlockCopy(embedding, 0, bytes, 0, bytes.Length);
        return bytes;
    }

    public float[] BytesToEmbedding(byte[] bytes)
    {
        if (bytes.Length != EmbeddingDimension * sizeof(float))
            throw new ArgumentException(
                $"Invalid byte length. Expected {EmbeddingDimension * sizeof(float)}, got {bytes.Length}.");

        var embedding = new float[EmbeddingDimension];
        Buffer.BlockCopy(bytes, 0, embedding, 0, bytes.Length);
        return embedding;
    }

    public void Dispose()
    {
        _session?.Dispose();
        _loadLock.Dispose();
    }

    // ───────────────────── Private helpers ─────────────────────

    /// <summary>
    /// Decode an image (JPEG/PNG byte array), resize to 112x112, and convert to
    /// a normalized NCHW float tensor for ArcFace input.
    /// </summary>
    private static DenseTensor<float> PreprocessImage(byte[] imageData)
    {
        using var bitmap = SKBitmap.Decode(imageData);
        if (bitmap is null)
            throw new ArgumentException("Failed to decode image data.");

        // Resize to 112x112 using high-quality filter
        using var resized = bitmap.Resize(new SKImageInfo(ImageSize, ImageSize), SKFilterQuality.High);
        if (resized is null)
            throw new InvalidOperationException("Failed to resize image.");

        // Build NCHW tensor: [1, 3, 112, 112]
        // ArcFace normalization: (pixel / 255.0 - 0.5) / 0.5 = pixel / 127.5 - 1.0
        var tensor = new DenseTensor<float>([1, 3, ImageSize, ImageSize]);

        for (int y = 0; y < ImageSize; y++)
        {
            for (int x = 0; x < ImageSize; x++)
            {
                var pixel = resized.GetPixel(x, y);
                tensor[0, 0, y, x] = (pixel.Red / 127.5f) - 1.0f;   // R
                tensor[0, 1, y, x] = (pixel.Green / 127.5f) - 1.0f;  // G
                tensor[0, 2, y, x] = (pixel.Blue / 127.5f) - 1.0f;   // B
            }
        }

        return tensor;
    }

    /// <summary>Cosine similarity between two vectors. Both should be L2-normalized for best results.</summary>
    private static float CosineSimilarity(float[] a, float[] b)
    {
        float dot = 0f, normA = 0f, normB = 0f;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        var denominator = MathF.Sqrt(normA) * MathF.Sqrt(normB);
        return denominator > 0f ? dot / denominator : 0f;
    }

    /// <summary>In-place L2 normalization of a vector.</summary>
    private static void L2Normalize(float[] vector)
    {
        float norm = 0f;
        for (int i = 0; i < vector.Length; i++)
        {
            norm += vector[i] * vector[i];
        }

        norm = MathF.Sqrt(norm);
        if (norm > 0f)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] /= norm;
            }
        }
    }
}
