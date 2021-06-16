using System;
using System.Collections.Generic;
using UnityEngine;
using static RocketMan.Grapher;

namespace RocketMan
{
    public class GraphPointCollection
    {
        private readonly GraphPoint[] points = new GraphPoint[Grapher.GraphMaxPointsNum];

        private int currentIndex = 0;

        private int currentPosition = 0;

        private int currentLength = 0;

        private float curdY = 0;

        public GraphPoint First
        {
            get => points[currentPosition];
        }

        public IEnumerable<GraphPoint> Points
        {
            get => GetPoints();
        }

        private float _minY = float.MaxValue;

        public float MinY
        {
            get => _minY;
        }

        private float _maxY = float.MinValue;

        public float MaxY
        {
            get => _maxY;
        }

        private float _minT = float.MaxValue;

        public float MinT
        {
            get => _minT;
        }

        private float _maxT = float.MinValue;

        public float MaxT
        {
            get => _maxT;
        }

        public float RangeT
        {
            get => MaxT - MinT;
        }

        public float RangeY
        {
            get => MaxY - MinY;
        }

        public int Index
        {
            get => currentIndex;
        }

        public int Length
        {
            get => currentLength;
        }

        public GraphPointCollection()
        {
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new GraphPoint(0, 0, new Color(1f, 1f, 1f, 0.1f));
            }
            currentLength = points.Length;
        }

        public void Add(GraphPoint point, bool dirty = true)
        {
            GraphPoint lastPoint = points[(currentPosition + Length - 1) % points.Length];
            GraphPoint sampledPoint = new GraphPoint();
            sampledPoint.t = point.t;
            sampledPoint.y = point.y;
            sampledPoint.color = point.color;
            float dY = (sampledPoint.y - lastPoint.y) / (sampledPoint.t - lastPoint.t);
            if ((Mathf.Abs(dY - curdY) < 1e-2 || Mathf.Abs(sampledPoint.y - lastPoint.y) < 1e-5))
            {
                points[(currentPosition + Length - 1) % points.Length] = sampledPoint;

                if (_minY > sampledPoint.y)
                    _minY = sampledPoint.y;
                if (_maxY < sampledPoint.y)
                    _maxY = sampledPoint.y;
                _maxT = sampledPoint.t;
            }
            else
            {
                points[currentPosition] = sampledPoint;
                currentPosition = (currentPosition + 1) % points.Length;
                curdY = dY;
                if (dirty)
                {
                    Dirty();
                }
            }
        }

        public void Dirty()
        {
            _minT = points[currentPosition].t;
            _maxT = points[(currentPosition + Length - 1) % points.Length].t;

            _minY = float.MaxValue;
            _maxY = float.MinValue;

            for (int i = 0; i < Length; i++)
            {
                if (points[i].y < _minY)
                    _minY = points[i].y;
                if (points[i].y > _maxY)
                    _maxY = points[i].y;
            }
        }

        private IEnumerable<GraphPoint> GetPoints()
        {
            for (int i = 0; i < Length; i++)
            {
                yield return points[(currentPosition + i) % points.Length];
            }
        }
    }
}
