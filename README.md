# tc.prober

This is an experimental project that aims to open a discussion about a possibility of unit testing of TwinCAT3 code using unit testing frameworks (such as NUnit, XUnit, etc.) to run testing code and make assertions. This approach would bring the advantage of using well-evolved testing frameworks and tools in unit testing of the plc code.

This project is using:

- Beckhoffs' TwinCAT3 4024.10
- VS2019
- Inxton.Package.Vortex.Core (referenced as NuGet package)
- NUnit

**TL;DR Inxton licensing**

Developer license is free and grants full functionality. It limits the run of the program to a period of up to 2 hours. After this period, the restart is required. You can get the license at inxton.com. **If do not want to register and you are contributing to https://github.com/dhullett08/TcOpen ping me to get license without registration**. 

## Concept

There a few enablers that make this discussion possible:

- The first is the possibility of invoking plc methods over ads.
- Invoking these methods from .net environment with plc method signature and return value.

*Example plc method*

~~~ Pascal
{attribute 'TcRpcEnable'}
METHOD RunCount : UINT
VAR_INPUT
	resetCounter : BOOL;
END_VAR
VAR_INST
	runs : UINT;	
END_VAR
-------------------------------------------------
IF(resetCounter) THEN runs := 0; RETURN; END_IF; 

runs := runs + 1;
RunCount := runs;
~~~

> Notice the attribute 'TcRpcEnable' that makes this method eligible to be Rpc called.

*Calling the method from C#*

~~~ C#
public ushort RunCount(System.Boolean resetCounter)
{ 
    return (ushort)Connector.InvokeRpc("Tests._basicRunnerTests", "RunCount", new object[]{resetCounter});
}
~~~

Unit testing

~~~ C#
[Test]
[TestCase((ushort)10)]
[TestCase((ushort)11)]
public void basic_runner_tests_run_count(ushort expected)
{
    //-- Arrange
    var sut = Entry.Plc.Tests._basicRunnerTests;
    sut.RunCount(true);

    //-- Act
    var actual = sut.Run((A) => A.RunCount(false), expected);

    //-- Assert
    Assert.AreEqual(expected, (ushort)actual);
}
~~~

## Assumptions

- It is possible to test single units of the plc program without running inside hard-real-time task of the plc system.
- The cyclical or event-driven engine can be managed from a non-real-time environment.

## Limitations

- The question arises around the interaction between hard-real-time and non-real-time, in particular when interacting with I/O systems. The units under tests should not be called from real-time, but the execution must be handled from non-real-time environment. I/O should be mirrored into data transfer variables/objects.
- Whenever the fast execution in order of us/ms with low jitter is required, this approach would be not suitable.
- When the execution is run by non-real-time task state of the program cannot be observed in TwinCAT3 editor.
