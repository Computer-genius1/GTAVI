using GTAVI.Windows.Models;
using GTAVI.Windows.Services;

namespace GTAVI.Tests;

public sealed class DiagnosticAssessmentServiceTests
{
    private readonly DiagnosticAssessmentService _service = new();

    [Fact]
    public void Evaluate_AllChecksHealthy_ReturnsHealthy()
    {
        var result = _service.Evaluate(new[]
        {
            new DiagnosticItem("Platform assessment", "Windows 11 x64"),
            new DiagnosticItem("Graphics driver health", "Adapter\n  Device status: OK (Code 0)"),
            new DiagnosticItem("Graphics API capabilities", "Direct3D 11: Available (Feature Level 12_1)\nDirect3D 12: Available")
        });

        Assert.Equal(DiagnosticHealthStatus.Healthy, result.Status);
        Assert.Equal(3, result.HealthyCount);
        Assert.Equal(0, result.NeedsAttentionCount);
        Assert.Equal(0, result.UnableToVerifyCount);
    }

    [Fact]
    public void Evaluate_DeviceError_ReturnsNeedsAttention()
    {
        var result = _service.Evaluate(new[]
        {
            new DiagnosticItem("Platform assessment", "Windows 11 x64"),
            new DiagnosticItem("Graphics driver health", "Adapter\n  Device status: Attention required (Code 43)"),
            new DiagnosticItem("Graphics API capabilities", "Direct3D 11: Available\nDirect3D 12: Available")
        });

        Assert.Equal(DiagnosticHealthStatus.NeedsAttention, result.Status);
        Assert.Equal(1, result.NeedsAttentionCount);
    }

    [Fact]
    public void Evaluate_ExperimentalPlatform_ReturnsUnableToVerifyWhenNoIssues()
    {
        var result = _service.Evaluate(new[]
        {
            new DiagnosticItem("Platform assessment", "Windows 11 ARM64 (experimental)"),
            new DiagnosticItem("Graphics driver health", "Adapter\n  Device status: OK (Code 0)"),
            new DiagnosticItem("Graphics API capabilities", "Direct3D 11: Available\nDirect3D 12: Available")
        });

        Assert.Equal(DiagnosticHealthStatus.UnableToVerify, result.Status);
        Assert.Equal(1, result.UnableToVerifyCount);
    }

    [Fact]
    public void Evaluate_MissingData_ReturnsUnableToVerify()
    {
        var result = _service.Evaluate(Array.Empty<DiagnosticItem>());

        Assert.Equal(DiagnosticHealthStatus.UnableToVerify, result.Status);
        Assert.Equal(3, result.UnableToVerifyCount);
    }

    [Fact]
    public void Evaluate_UnavailableDirect3DCheck_ReturnsNeedsAttention()
    {
        var result = _service.Evaluate(new[]
        {
            new DiagnosticItem("Platform assessment", "Windows 11 x64"),
            new DiagnosticItem("Graphics driver health", "Adapter\n  Device status: OK (Code 0)"),
            new DiagnosticItem("Graphics API capabilities", "Direct3D 11: Available\nDirect3D 12: Unavailable (HRESULT)")
        });

        Assert.Equal(DiagnosticHealthStatus.NeedsAttention, result.Status);
    }

    [Fact]
    public void Evaluate_NeedsAttentionTakesPrecedenceOverUnableToVerify()
    {
        var result = _service.Evaluate(new[]
        {
            new DiagnosticItem("Platform assessment", "Windows 11 ARM64 (experimental)"),
            new DiagnosticItem("Graphics driver health", "Adapter\n  Device status: Attention required (Code 43)"),
            new DiagnosticItem("Graphics API capabilities", "Direct3D 11: Available\nDirect3D 12: Available")
        });

        Assert.Equal(DiagnosticHealthStatus.NeedsAttention, result.Status);
        Assert.Equal(1, result.UnableToVerifyCount);
        Assert.Equal(1, result.NeedsAttentionCount);
    }
}
