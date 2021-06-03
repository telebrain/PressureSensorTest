using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwenPressureDevices;

namespace MetrologicGroupTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void SearshGroupTest()
        {
            MetrologicGroups mg = new MetrologicGroups(20101);
            int group = mg.GetMetrologicGroup("ПД100-ДИ0,6-111-0,5");
            if (group != 5)
                throw new Exception("Не та группа");
        }

        [TestMethod]
        public void BadRange()
        {
            try
            {
                MetrologicGroups mg = new MetrologicGroups(20101);
                int group = mg.GetMetrologicGroup("ПД100-ДИ6,0-111-0,5");
                throw new Exception("Не должен был ничего найти");
            }
            catch { }
        }

        [TestMethod]
        public void BadType()
        {
            try
            {
                MetrologicGroups mg = new MetrologicGroups(20101);
                int group = mg.GetMetrologicGroup("ПД100-ДВ1,0-111-0,5");
                throw new Exception("Не должен был ничего найти");
            }
            catch { }
        }

        [TestMethod]
        public void SearshDI()
        {
            MetrologicGroups mg = new MetrologicGroups(20102);
            int group = mg.GetMetrologicGroup("ПД100-ДИ0,6-111-0,5");
            if (group != 14)
                throw new Exception("Не та группа");
        }

        [TestMethod]
        public void SearshDD()
        {
            MetrologicGroups mg = new MetrologicGroups(20102);
            int group = mg.GetMetrologicGroup("ПД100-ДД0,6-111-0,5");
            if (group != 14)
                throw new Exception("Не та группа");
        }

        [TestMethod]
        public void SearshDG()
        {
            MetrologicGroups mg = new MetrologicGroups(20102);
            int group = mg.GetMetrologicGroup("ПД100-ДГ0,6-111-0,5");
            if (group != 14)
                throw new Exception("Не та группа");
        }

        [TestMethod]
        public void SearshDIV1()
        {
            MetrologicGroups mg = new MetrologicGroups(20102);
            int group = mg.GetMetrologicGroup("ПД100-ДИВ1,5-111-0,5");
            if (group != 16)
                throw new Exception("Не та группа");
        }

        [TestMethod]
        public void SearshDIV2()
        {
            MetrologicGroups mg = new MetrologicGroups(20102);
            int group = mg.GetMetrologicGroup("ПД100-ДИВ0,1-111-0,5");
            if (group != 13)
                throw new Exception("Не та группа");
        }
        [TestMethod]
        public void SearshDV()
        {
            MetrologicGroups mg = new MetrologicGroups(20102);
            int group = mg.GetMetrologicGroup("ПД100-ДВ0,1-111-0,5");
            if (group != 13)
                throw new Exception("Не та группа");
        }

        [TestMethod]
        public void SearshDA()
        {
            MetrologicGroups mg = new MetrologicGroups(20102);
            int group = mg.GetMetrologicGroup("ПД100-ДА1,6-111-0,5");
            if (group != 16)
                throw new Exception("Не та группа");
        }
    }
}
