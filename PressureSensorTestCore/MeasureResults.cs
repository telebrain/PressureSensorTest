﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureSensorTestCore
{
    public class MeasureResults: IEnumerable
    {
        // Все значения давлений в Па

        public int Count
        {
            get { return checkPoints.Count; }
        }

        List<CheckPoint> checkPoints = new List<CheckPoint>();
        

        public CheckPoint this[int index]
        {
            get
            {
                if (index >= 0 && index < checkPoints.Count)
                {
                    return checkPoints[index];
                }
                else
                {
                    return null;
                }
            }
        }

        public CheckPoint GetCheckPointByPercent(int percentRange)
        {
            foreach(var point in checkPoints)
            {
                if (point.PercentRange == percentRange)
                    return point;
            }
            return null;
        }

        public IEnumerator GetEnumerator()
        {
            return checkPoints.GetEnumerator();
        }

        public void Add(CheckPoint checkPoint)
        {
            checkPoints.Add(checkPoint);
        }

        public void Clear()
        {
            checkPoints.Clear();
        }

        public bool? GetResume()
        {
            if (checkPoints.Count == 0)
                return null;
            bool? res = true;
            foreach (var point in checkPoints)
            {
                res &= point.Resume;
            }
            return res;
        }
    }
}
