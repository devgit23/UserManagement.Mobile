namespace UserManagement.Mobile.Core.Services.Interfaces;

/// <summary>
/// On-device face recognition using ONNX Runtime with ArcFace model.
/// Handles embedding generation and cosine-similarity matching.
/// </summary>
public interface IFaceRecognitionService
{
    /// <summary>Whether the ONNX model is loaded and ready for inference.</summary>
    bool IsModelLoaded { get; }

    /// <summary>Load the ArcFace ONNX model into memory. Call once at startup or first use.</summary>
    Task LoadModelAsync();

    /// <summary>Dispose the loaded model to free memory.</summary>
    void UnloadModel();

    /// <summary>
    /// Generate a 512-dimensional face embedding from a cropped, aligned face image.
    /// The image should be a JPEG/PNG byte array of a face region (ideally 112x112).
    /// </summary>
    Task<float[]> GenerateEmbeddingAsync(byte[] faceImageData);

    /// <summary>
    /// Compare two face embeddings using cosine similarity.
    /// Returns a match result with confidence score.
    /// </summary>
    FaceMatchResult Verify(float[] capturedEmbedding, float[] enrolledEmbedding, float threshold = 0.55f);

    /// <summary>
    /// Average multiple embeddings into a single robust template (used during enrollment).
    /// </summary>
    float[] AverageEmbeddings(IReadOnlyList<float[]> embeddings);

    /// <summary>
    /// Convert a float[] embedding to a byte[] for storage/transport (little-endian float32).
    /// </summary>
    byte[] EmbeddingToBytes(float[] embedding);

    /// <summary>
    /// Convert a stored byte[] back to a float[] embedding.
    /// </summary>
    float[] BytesToEmbedding(byte[] bytes);
}

/// <summary>Result of a face verification comparison.</summary>
public sealed class FaceMatchResult
{
    public bool IsMatch { get; init; }

    /// <summary>Cosine similarity score (0.0 – 1.0).</summary>
    public float Confidence { get; init; }

    /// <summary>Human-readable failure reason, if any.</summary>
    public string? FailureReason { get; init; }
}
