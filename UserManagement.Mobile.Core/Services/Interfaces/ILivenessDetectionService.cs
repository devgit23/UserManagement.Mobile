namespace UserManagement.Mobile.Core.Services.Interfaces;

/// <summary>
/// Anti-spoofing liveness detection service.
/// Analyzes face images to distinguish live faces from photos, screens, or masks.
/// </summary>
public interface ILivenessDetectionService
{
    /// <summary>
    /// Start a new liveness verification session with a timeout.
    /// Must be called before submitting frames.
    /// </summary>
    void StartSession(TimeSpan timeout);

    /// <summary>Whether the current session has expired.</summary>
    bool IsSessionExpired { get; }

    /// <summary>
    /// Submit a captured frame for liveness analysis.
    /// Multiple frames are needed to detect blink and head movement.
    /// </summary>
    Task<LivenessFrameResult> AnalyzeFrameAsync(byte[] imageData);

    /// <summary>
    /// Get the overall liveness verdict after multiple frames have been analyzed.
    /// Requires at least 3 frames submitted via <see cref="AnalyzeFrameAsync"/>.
    /// </summary>
    LivenessVerdict GetVerdict();

    /// <summary>
    /// Generate a one-time session nonce for replay attack prevention.
    /// Include this nonce in the attendance clock request.
    /// </summary>
    string GenerateSessionNonce();

    /// <summary>Reset the session state for a new verification attempt.</summary>
    void Reset();
}

/// <summary>Result of analyzing a single frame for liveness signals.</summary>
public sealed class LivenessFrameResult
{
    /// <summary>Whether this individual frame passes basic quality checks.</summary>
    public bool IsAcceptable { get; init; }

    /// <summary>Texture variance score — low values indicate a flat/printed surface.</summary>
    public double TextureVariance { get; init; }

    /// <summary>Estimated face sharpness (edge response).</summary>
    public double Sharpness { get; init; }

    /// <summary>Guidance message for the user ("Blink naturally", "Hold still", etc.).</summary>
    public string GuidanceText { get; init; } = string.Empty;
}

/// <summary>Overall liveness verdict after analyzing multiple frames.</summary>
public sealed class LivenessVerdict
{
    /// <summary>Whether the subject is determined to be a live person.</summary>
    public bool IsLive { get; init; }

    /// <summary>Overall liveness confidence (0.0 – 1.0).</summary>
    public float Confidence { get; init; }

    /// <summary>Human-readable failure reason, if not live.</summary>
    public string? FailureReason { get; init; }

    /// <summary>Whether blink was detected across frames.</summary>
    public bool BlinkDetected { get; init; }

    /// <summary>Whether micro head movement was detected across frames.</summary>
    public bool MovementDetected { get; init; }

    /// <summary>Whether texture analysis passed (not a printed photo).</summary>
    public bool TexturePassed { get; init; }
}
