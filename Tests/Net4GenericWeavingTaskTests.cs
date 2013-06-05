using NUnit.Framework;

[TestFixture]
public class Net4GenericWeavingTaskTests : BaseTaskTests
{

    public Net4GenericWeavingTaskTests()
        : base(@"AssemblyToProcess\AssemblyToProcessGenericDotNet4.csproj")
    {

    }


    [Test]
    public void DataErrorInfo()
    {
        var instance = Assembly.GetInstance("Person");
        ValidationTester.TestDataErrorInfo(instance);
    }
    [Test]
    public void DataErrorInfoWithImplementation()
    {
        var instance = Assembly.GetInstance("PersonWithImplementation");
        ValidationTester.TestDataErrorInfo(instance);
    }

}