namespace Tc.Prober.RecorderTests
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using PlcTcProberTests;
    using System.IO;
    using Vortex.Connector;
    using System.Reflection;
    using Tc.Prober.Recorder;
    using Vortex.Adapters.Connector.Tc3.Adapter;

    [TestFixture()]
    public class RunnerTests
    {
        private PlcTcProberTestsTwinController  connector = null;
        private string _runner_recording_file;
        private string _runner_with_test_method_file;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {            
            Runner.RecordingsShell = Path.GetFullPath(Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, @"..\..\..\output\shell\"));            
            connector = new PlcTcProberTestsTwinController(Tc3ConnectorAdapter.Create(null, 851, false));
            connector.Connector.BuildAndStart().ReadWriteCycleDelay = 10;
        }

        [Test()]
        public void RunTest()
        {           
            //-- Act
            int iterationCount = 0;
            int openCalsCount = 0;
            int closeCallCount = 0;
            connector.MAIN.Run(() => true, 
                               () => { iterationCount++; return iterationCount < 10; }, 
                               () => openCalsCount++, 
                               () => closeCallCount++);

            //-- Assert
            Assert.AreEqual(10, iterationCount);           
            Assert.AreEqual(9, openCalsCount);
            Assert.AreEqual(9, closeCallCount);

        }

        [Test()]
        [Order(100)]
        public void RunWithRecordingTest()
        {
           
            //-- Act
            int iterationCount = 0;
            int openCalsCount = 0;
            int closeCallCount = 0;
            connector.MAIN.Run(() => true,
                             () => { iterationCount++; return iterationCount < 10; },                          
                             () => openCalsCount++, 
                             () => closeCallCount++,                            
                             new Recorder<fbInheritanceLevel_5, PlainfbInheritanceLevel_5>(connector.MAIN.InheritanceRw, RecorderModeEnum.Graver, 0).Actor,
                             Recorder.Runner.GetAutoMethodName(2));

            //-- Assert
            Assert.AreEqual(10, iterationCount);
            Assert.AreEqual(9, openCalsCount);
            Assert.AreEqual(9, closeCallCount);

        }


        //[Test()]
        //[Order(200)]
        //public void RecordTestStructureTest()
        //{
        //    var sut = connector.Tests._recorderRunnerTests;
        //    int iterationCount = 0;
        //    sut.Run(() => sut.RunWithRecorder(),
        //            () => { iterationCount++; return iterationCount > 100; },
        //            null,
        //            null,
        //            new Recorder.Recorder<stRecorder, PlainstRecorder>(sut._recorder, RecorderModeEnum.Graver).Actor,
        //            Path.Combine(Runner.RecordingsShell, Runner.CallerMethodName())
        //            );
        //}

        //[Test()]
        //[Order(300)]
        //public void RunWithReplayingTest()
        //{
          
        //}
    }
}