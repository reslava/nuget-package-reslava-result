namespace REslava.Result.Tests;

[TestClass]
public sealed class Result_Creation
{
    [TestMethod]
    [DataRow("")]
    [DataRow("message1")]
    public void Result_Factories_Ok_string(string message)
    {
        var result = Result.Ok().WithSuccess(message);
        Assert.AreEqual(message, result.Reasons[0].Message);
        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.IsFailed);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("message1")]
    public void Result_int_Factories_Ok_string(string message)
    {
        var result = Result<int>.Ok().WithSuccess(message);
        Assert.AreEqual(message, result.Reasons[0].Message);
        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.IsFailed);
    }
    [TestMethod]
    [DataRow(0, "")]
    [DataRow(1, "message1")]
    [DataRow(-1, "message 2")]
    public void Result_int_Factories_Ok_int_string(int value, string message)
    {
        var result = Result<int>.Ok(value).WithSuccess(message);
        Assert.AreEqual(message, result.Reasons[0].Message);
        Assert.AreEqual(value, result.ValueOrDefault);
        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.IsFailed);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("message1")]

    public void Result_Factories_Fail_string(string message)
    {
        var result = Result.Fail(message);
        Assert.AreEqual(message, result.Reasons[0].Message.ToString());
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.IsFailed);
    }
}
