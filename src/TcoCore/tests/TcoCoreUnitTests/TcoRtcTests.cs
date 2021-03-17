using NUnit.Framework;
using System;
using System.Text;
using System.Threading;

namespace TcoCoreUnitTests
{

    public class T00_TcoRtcTests
    {

        TcoCoreTests.TcoRtcTest tc_A = TcoCoreUnitTests.ConnectorFixture.Connector.MAIN._TcoRtcTest_A;
        TcoCoreTests.TcoRtcTest tc_B = TcoCoreUnitTests.ConnectorFixture.Connector.MAIN._TcoRtcTest_B;
        int Delay = 500;

        [SetUp]
        public void Setup()
        {
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
        }


        [Test, Order(000)]
        public void T000_SetSynchronizationParametersFromTcA()
        {
            //This test triggers set synchronization parameters.
            //This test should be very first, so it should set the SynchronizationContextIndentity to the Identity of this test instance.
            //No other instance should be able to overwrite this Identity so as the other synchronization parameters.
            //The difference between the behavior of the very first test run after PLC download and the next test run is only the assignment of the SynchronizationContextIndentity
            //and immediately start the synchronization sequence.
            //So for all the other test runs waiting for the "falling edge" of the tc_A.RtcIsValid() is skipped, otherwise the test should take a lot time based on the SyncPeriod value and
            //time of the test start.
            string synchAmsId = TwinCAT.Ads.AmsNetId.Local.ToString();
            ushort SyncPeriod = 60;
            bool isFirstTestRunAfterPlcDownload = false;
            tc_A.GetSynchParams();

            isFirstTestRunAfterPlcDownload = tc_A._RtcSynchParams.synchContextIdentity.Synchron == 0;
            
            tc_A.SetSynchParams(true, synchAmsId, SyncPeriod);

            tc_A._CallMyPlcInstance.Synchron = true;            //Switch on the cyclical execution of the _TcoRtcTest_A instance 

            if (isFirstTestRunAfterPlcDownload)
            {
                while (tc_A.RtcIsValid()) { } 
            }

            while (!tc_A.RtcIsValid()) { }
            tc_A._CallMyPlcInstance.Synchron = false;           //Switch off the cyclical execution of the _TcoRtcTest_A instance 

            tc_A.GetSynchParams();

            Assert.AreEqual(tc_A._MyIdentity.Synchron, tc_A._RtcSynchParams.synchContextIdentity.Synchron);
        }

        [Test, Order(001)]
        public void T001_SetSynchronizationParametersFromTcB()
        {
            //This test triggers set synchronization parameters again from different TcoContext to the different values.
            //The synchronization parameters should not change, as it is possible only from tc_A as it was the very first TcoContext instance called SetSynchParams() method.
            
            tc_A.GetSynchParams();

            Assert.AreNotEqual(0, tc_A._RtcSynchParams.synchContextIdentity.Synchron);

            ulong synchContextIdentity = tc_A._RtcSynchParams.synchContextIdentity.Synchron;
            string synchAmsId = tc_A._RtcSynchParams.syncAmsId.Synchron;
            ushort SyncPeriod = tc_A._RtcSynchParams.syncPeriod.Synchron;
            bool doSync = tc_A._RtcSynchParams.doSynch.Synchron;
            string newSyncAmsId = TestHelpers.RandomString(10);

            tc_B.SetSynchParams(!doSync, newSyncAmsId, (ushort)(2 * SyncPeriod));

            tc_B._CallMyPlcInstance.Synchron = true;            //Switch on the cyclical execution of the _TcoRtcTest_B instance 

            while (!tc_B.RtcIsValid()) { }
            tc_B._CallMyPlcInstance.Synchron = false;           //Switch off the cyclical execution of the _TcoRtcTest_B instance 

            tc_B.GetSynchParams();

            Assert.AreNotEqual(tc_B._MyIdentity.Synchron, tc_B._RtcSynchParams.synchContextIdentity.Synchron);
            Assert.AreEqual(synchContextIdentity, tc_B._RtcSynchParams.synchContextIdentity.Synchron);
            Assert.AreEqual(synchAmsId, tc_B._RtcSynchParams.syncAmsId.Synchron);
            Assert.AreEqual(SyncPeriod, tc_B._RtcSynchParams.syncPeriod.Synchron);
            Assert.AreEqual(doSync, tc_B._RtcSynchParams.doSynch.Synchron);
        }


        [Test, Order(002)]
        public void T002_ChangeSynchronizationParametersFromTcA()
        {
            //This test triggers set synchronization parameters again from the sama TcoContext as first time to the different values.
            //The synchronization parameters should change.

            tc_A.GetSynchParams();

            Assert.AreNotEqual(0, tc_A._RtcSynchParams.synchContextIdentity.Synchron);

            ulong synchContextIdentity = tc_A._RtcSynchParams.synchContextIdentity.Synchron;
            string synchAmsId = tc_A._RtcSynchParams.syncAmsId.Synchron;
            ushort SyncPeriod = tc_A._RtcSynchParams.syncPeriod.Synchron;
            bool doSync = tc_A._RtcSynchParams.doSynch.Synchron;
            string newSyncAmsId = TestHelpers.RandomString(10);

            tc_A.SetSynchParams(!doSync, newSyncAmsId, (ushort)(2 * SyncPeriod));

            tc_A._CallMyPlcInstance.Synchron = true;            //Switch on the cyclical execution of the _TcoRtcTest_B instance 

            while (!tc_A.RtcIsValid()) { }
            tc_A._CallMyPlcInstance.Synchron = false;           //Switch off the cyclical execution of the _TcoRtcTest_B instance 

            tc_A.GetSynchParams();

            Assert.AreEqual(tc_A._MyIdentity.Synchron, tc_A._RtcSynchParams.synchContextIdentity.Synchron);
            Assert.AreEqual(synchContextIdentity, tc_A._RtcSynchParams.synchContextIdentity.Synchron);
            Assert.AreNotEqual(synchAmsId, tc_A._RtcSynchParams.syncAmsId.Synchron);
            Assert.AreNotEqual(SyncPeriod, tc_A._RtcSynchParams.syncPeriod.Synchron);
            Assert.AreNotEqual(doSync, tc_A._RtcSynchParams.doSynch.Synchron);


            tc_A.SetSynchParams(doSync, synchAmsId, SyncPeriod);

            tc_A._CallMyPlcInstance.Synchron = true;            //Switch on the cyclical execution of the _TcoRtcTest_B instance 

            while (!tc_A.RtcIsValid()) { }
            tc_A._CallMyPlcInstance.Synchron = false;           //Switch off the cyclical execution of the _TcoRtcTest_B instance 

            tc_A.GetSynchParams();

            Assert.AreEqual(tc_A._MyIdentity.Synchron, tc_A._RtcSynchParams.synchContextIdentity.Synchron);
            Assert.AreEqual(synchContextIdentity, tc_A._RtcSynchParams.synchContextIdentity.Synchron);
            Assert.AreEqual(synchAmsId, tc_A._RtcSynchParams.syncAmsId.Synchron);
            Assert.AreEqual(SyncPeriod, tc_A._RtcSynchParams.syncPeriod.Synchron);
            Assert.AreEqual(doSync, tc_A._RtcSynchParams.doSynch.Synchron);


        }
        [Test, Order(002)]
        public void T002_RtcIsValid()
        {
            tc_A._CallMyPlcInstance.Synchron = true;            //Switch on the cyclical execution of the _TcoContextTest_A instance             

            Thread.Sleep(Delay);                                //Time of the cyclical execution of the _TcoContextTest_A instance

            tc_A._CallMyPlcInstance.Synchron = false;           //Switch off the cyclical execution of the _TcoContextTest_A instance 

            Assert.IsTrue(tc_A.RtcIsValid());                   //System time should already been read out and valid, as PLC instance was running at least Delay time.
        }

        [Test, Order(003)]
        public void T003_RtcNowLocalChanging()
        {

            Delay = 5000;                                       //Set delay to 5000ms
            tc_A._CallMyPlcInstance.Synchron = true;            //Switch on the cyclical execution of the _TcoContextTest_A instance       
            tc_A.NowLocal();
            DateTime _startPLCTime = tc_A._NowLocal.Synchron;   //Readout actual time of the instance (number of seconds from 1.1.1970).
            DateTime _startDotNetTime = DateTime.Now;
            Thread.Sleep(Delay);                                //Time of the cyclical execution of the task instance

            tc_A._CallMyPlcInstance.Synchron = false;           //Switch off the cyclical execution of the _TcoContextTest_A instance 

            tc_A.NowLocal();
            DateTime _endPLCTime = tc_A._NowLocal.Synchron;     //Readout actual time of the instance (number of seconds from 1.1.1970). Does not matter that .Net System.DateTime starts in 1.1.0001
            DateTime _endDotNetTime = DateTime.Now;

            TimeSpan _PlcDuration = _endPLCTime - _startPLCTime;
            TimeSpan _DotNetDuration = _endDotNetTime - _startDotNetTime;
            Assert.AreEqual(Delay, _PlcDuration.TotalMilliseconds); //Check if time difference is equal.

            Assert.LessOrEqual(Math.Abs(_PlcDuration.TotalMilliseconds - _DotNetDuration.TotalMilliseconds), 1000);
        }

        [Test, Order(004)]
        public void T004_RtcNowUtcChanging()
        {

            Delay = 5000;                                       //Set delay to 5000ms
            tc_A._CallMyPlcInstance.Synchron = true;            //Switch on the cyclical execution of the _TcoContextTest_A instance       
            tc_A.NowUtc();
            DateTime _startPLCTime = tc_A._NowUtc.Synchron;   //Readout actual time of the instance (number of seconds from 1.1.1970).
            DateTime _startDotNetTime = DateTime.UtcNow;
            Thread.Sleep(Delay);                                //Time of the cyclical execution of the task instance

            tc_A._CallMyPlcInstance.Synchron = false;           //Switch off the cyclical execution of the _TcoContextTest_A instance 

            tc_A.NowUtc();
            DateTime _endPLCTime = tc_A._NowUtc.Synchron;     //Readout actual time of the instance (number of seconds from 1.1.1970). Does not matter that .Net System.DateTime starts in 1.1.0001
            DateTime _endDotNetTime = DateTime.UtcNow;

            TimeSpan _PlcDuration = _endPLCTime - _startPLCTime;
            TimeSpan _DotNetDuration = _endDotNetTime - _startDotNetTime;
            Assert.AreEqual(Delay, _PlcDuration.TotalMilliseconds); //Check if time difference is equal.

            Assert.LessOrEqual(Math.Abs(_PlcDuration.TotalMilliseconds - _DotNetDuration.TotalMilliseconds), 1000);
        }

        [Test, Order(005)]
        public void T005_RtcTickClockChanging()
        {
            string _sStartTime = "";
            Delay = 5000;                                        //Set delay to 500ms
            tc_A._CallMyPlcInstance.Synchron = true;            //Switch on the cyclical execution of the _TcoContextTest_A instance             
            if (_sStartTime == "")
            {
                //Readout actual time of the instance (number of hundreds of nanoseconds from 1.1.1970).
                _sStartTime = new StringBuilder(tc_A.TickClock()) { [10] = 'T' }.ToString();
            }

            Thread.Sleep(Delay);                                //Time of the cyclical execution of the task instance

            tc_A._CallMyPlcInstance.Synchron = false;           //Switch off the cyclical execution of the _TcoContextTest_A instance 

            //Readout actual time of the instance (number of hundreds of nanoseconds from 1.1.1970).
            string _sEndTime = new StringBuilder(tc_A.TickClock()) { [10] = 'T' }.ToString();

            DateTime _dtStartTime;
            DateTime _dtEndTime;
            DateTime.TryParse(_sStartTime, out _dtStartTime);
            DateTime.TryParse(_sEndTime, out _dtEndTime);
            TimeSpan _diff = _dtEndTime - _dtStartTime;
            Assert.GreaterOrEqual(_diff.TotalMilliseconds, Delay * 0.9);
            Assert.LessOrEqual(_diff.TotalMilliseconds, Delay * 1.1);
        }

        [Test, Order(006)]
        public void T006_RtcTickClockDiff()
        {

            tc_A._CallMyPlcInstance.Synchron = true;            //Switch on the cyclical execution of the _TcoContextTest_A instance             
            //Readout actual time of the instance (number of hundreds of nanoseconds from 1.1.1970).
            string _splcTime = new StringBuilder(tc_A.TickClock()) { [10] = 'T' }.ToString();
            tc_A._CallMyPlcInstance.Synchron = false;           //Switch off the cyclical execution of the _TcoContextTest_A instance 
            DateTime _dtplcTime;
            DateTime.TryParse(_splcTime, out _dtplcTime);
            DateTime _dotNetTime = DateTime.UtcNow;
            TimeSpan _diff = _dotNetTime - _dtplcTime;
            Assert.LessOrEqual(Math.Abs(_diff.TotalMilliseconds), 1000);
        }


    }
}