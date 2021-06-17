using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
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

        private float maxTWithoutAddtion = 60;

        private float curdeltaY = 0;

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

        public float MaxTWithoutAddtion
        {
            get => maxTWithoutAddtion;
            set => maxTWithoutAddtion = value;
        }

        public GraphPointCollection()
        {
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new GraphPoint(0, 0, new Color(1f, 1f, 1f, 0.1f));
            }
            currentLength = 0;
        }

        public void Add(GraphPoint point, bool dirty = true)
        {
            if (currentLength == 0)
            {
                points[0] = point;
                currentPosition = 1;
                currentLength = 1;
                Dirty();
            }
            else
            {
                GraphPoint lastPoint = points[(currentPosition + Length - 1) % points.Length];
                float deltaY = (point.y - point.y) / (point.t - lastPoint.t);

                if (Length == points.Length
                    && lastPoint.t - points[(currentPosition + Length - 2) % points.Length].t < maxTWithoutAddtion
                    && (Mathf.Abs(deltaY - curdeltaY) < 1e-2 || Mathf.Abs(point.y - lastPoint.y) < 1e-5)
                    && point.color == lastPoint.color)
                {
                    points[(currentPosition + Length - 1) % points.Length] = point;

                    if (_minY > point.y)
                        _minY = point.y;
                    if (_maxY < point.y)
                        _maxY = point.y;
                    _maxT = point.t;
                }
                else
                {
                    points[currentPosition] = point;
                    currentPosition = (currentPosition + 1) % points.Length;
                    currentLength = Math.Min(currentLength + 1, points.Length);
                    curdeltaY = deltaY;
                    if (dirty)
                    {
                        Dirty();
                    }
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
            if (Length == points.Length)
            {
                for (int i = 0; i < Length; i++)
                {
                    yield return points[(currentPosition + i) % points.Length];
                }
            }
            else
            {
                float t = points[0].t;
                for (int i = 0; i < Length; i++)
                {
                    GraphPoint p = points[i];
                    if (t < p.t)
                    {
                        t = p.t;
                        yield return p;
                    }
                }
            }
        }
    }
}
