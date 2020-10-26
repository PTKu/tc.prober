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
            _runner_recording_file = Path.Combine(Runner.RecordingsShell, $"{nameof(RunWithRecordingTest)}_{nameof(RunWithReplayingTest)}");
            _runner_with_test_method_file = Path.Combine(Runner.RecordingsShell, $"{nameof(RunRecordingWithTestMethod)}_{nameof(RunReplayWithTestMethod)}");


#if DEBUG
            var adapter = Vortex.Adapters.Connector.Tc3.Adapter.Tc3ConnectorAdapter.Create(null, 851, false);
            // ConnectorTests.ConnectorTestsTwinController.LocalizationDirectory = @"C:\MTS\Develop\vts\vortex.builder\_Vortex\out\ConnectorTests\loc\";
            connector = new ConnectorTests.ConnectorTestsTwinController(adapter);        
            connector.Connector.ReadWriteCycleDelay = 10;
            connector.Connector.BuildAndStart();
#else
            connector = new PlcTcProberTestsTwinController();
#endif
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
                             _runner_recording_file);

            //-- Assert
            Assert.AreEqual(10, iterationCount);
            Assert.AreEqual(9, openCalsCount);
            Assert.AreEqual(9, closeCallCount);

        }
#if DEBUG
        [Test]
        [Order(200)]
        public void RunWithReplayingTest()
        {
            //-- Arrange
            var runner = new Runner();           

            //-- Act
            int iterationCount = 0;
            int openCalsCount = 0;
            int closeCallCount = 0;
            runner.Run<bool>(() => true,
                             () => { iterationCount++; return iterationCount < 10; },
                             _runner_recording_file,
                             () => openCalsCount++,
                             () => closeCallCount++,
                             new Recorder<fbInheritanceLevel_5, PlainfbInheritanceLevel_5>(connector.MAIN.InheritanceRw, RecorderModeEnum.Player).Actor);

            //-- Assert
            Assert.AreEqual(10, iterationCount);
            Assert.AreEqual(9, openCalsCount);
            Assert.AreEqual(9, closeCallCount);

        }

        [Test()]
        [Order(1000)]
        public void RunRecordingWithTestMethod()
        {
            //-- Arrange
            var runner = new Runner();
            connector.MAIN.Recorder.runCount.Synchron = 0;
            bool ended = false;

            foreach (var item in connector.MAIN.Recorder.In.Inputs)
            {
                item.Synchron = 0;
            }

            //-- Act           
            runner.Run<bool>(() => ended = connector.MAIN.Recorder.TestMethod(true),
                             () => !ended,                             
                             _runner_with_test_method_file,
                             () => System.Threading.Thread.Sleep(10),
                             recorder:new Recorder<stRecorderInputs, PlainstRecorderInputs>(connector.MAIN.Recorder.In, RecorderModeEnum.Graver).Actor);
            
        }

        [Test()]
        [Order(1100)]
        public void RunReplayWithTestMethod()
        {
            //-- Arrange
            var runner = new Runner();
            connector.MAIN.Recorder.runCount.Synchron = 0;

            //-- Act
            bool ended = false;
            int counts = 0;
            int index = 0;
            runner.Run<bool>(() => ended = connector.MAIN.Recorder.TestMethod(false),
                             () => !ended,
                             _runner_with_test_method_file,
                             closeCycle:() =>
                             {                                 
                                
                                 if(counts > 1)
                                 {
                                     index++;
                                 }

                                 Console.WriteLine("----------------------------------------------");
                                 for (int i = 0; i < connector.MAIN.Recorder.In.Inputs.Length; i++)
                                 {
                                     Console.WriteLine(connector.MAIN.Recorder.In.Inputs[i].Synchron);
                                     Assert.AreEqual(i * index,
                                                    connector.MAIN.Recorder.In.Inputs[i].Synchron,
                                                    $"{i}:{index}");

                                     Assert.AreEqual(i * index,
                                                    connector.MAIN.Recorder.Out.Ouputs[i].Synchron,
                                                    $"{i}:{index}");
                                 }
                                 Console.WriteLine("----------------------------------------------");

                                 counts++;

                             },
                             recorder: new Recorder<stRecorderInputs, PlainstRecorderInputs>(connector.MAIN.Recorder.In, RecorderModeEnum.Player).Actor);

        }

#else        
        public void RunWithReplayingTest() { }
        public void RunRecordingWithTestMethod() { }
        public void RunReplayWithTestMethod() { }
#endif
    }
}