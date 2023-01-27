using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Kinect.Face;
using UnityEngine;
using Windows.Kinect;
using Kinect = Windows.Kinect;


public class FilterDexp
{
    float _smoothing;
    float _correction;
    float _prediction;
    float _jitterRadius;
    float _maxDeviationRadius;

    FilterDoubleExponentialData _history;

    // -------------------------------------------------------------------
    // -------------------------------------------------------------------
    public FilterDexp(FilterDexpParams parameters = null)
    {
        var parms = parameters;

        if (parms == null)
            parms = new FilterDexpParams();

        _smoothing = parms.Smoothing;
        _correction = parms.Correction;
        _prediction = parms.Prediction;

        // Check for divide by zero. Use an epsilon of a 10th of a millimeter
        _jitterRadius = Math.Max(0.0001f, parms.JitterRadius);
        _maxDeviationRadius = parms.MaxDeviationRadius;

        _history = new FilterDoubleExponentialData();
    }

    private void Reset()
    {
        _history = new FilterDoubleExponentialData();
    }

    //public Vector3 justEstimate()
    //{

    //}

    public Vector3 updateAndCorrect(Vector3 sample, bool extraJitterRadius = false)
    {
        // optional - smooth a bit more by using a bigger jitter radius
        var jitterRadius = _jitterRadius;
        var maxDeviationRadius = _maxDeviationRadius;
        if (extraJitterRadius)
        {
            jitterRadius *= 2.0f;
            maxDeviationRadius *= 2.0f;
        }

        //next values
        Vector3 filtered;        
        Vector3 trend;

        //prev values  
        var prevFiltered = _history.Filtered;
        var prevTrend = _history.Trend;
        var prevSample = _history.Sample;

        //temps 
        float diffVal;
        float w;

        if (this._history.FrameCount == 0)
        {
            // * first sample
            filtered = sample;
            trend = new Vector3();
        }
        else if (this._history.FrameCount == 1)
        {
            // * 2 samples 
            filtered = (sample + prevSample) * 0.5f;
            var newTrend = filtered - prevFiltered;
            trend = prevTrend + _correction * (newTrend - prevTrend);
        }
        else
        {
            // * running

            // ** apply jitter filter
            //    anything within a small radius is probably a jitter, not an actual change - inhibit to the last state...
            //    if it's far - this is probably a real change in value, just take it as is.
            diffVal = Vector3.Distance(sample, prevFiltered);
            if (diffVal <= jitterRadius)
            {
                w = (diffVal / jitterRadius);
                sample = prevFiltered + w * (sample - prevFiltered);
            }

            // ** apply double exponential smoothing filter
            //    predict state accroding to last state and trend, and weight it in (_smooth).
            //    update trend according to new state and prev state and weight it in (_correct).

            var expectedState = (prevFiltered + prevTrend);
            filtered = sample + _smoothing * (expectedState - sample);

            var correctedTrend = (filtered - prevFiltered);
            trend = prevTrend + _correction * (correctedTrend - prevTrend);
        }

        // ** extrapolate into the future to reduce latency
        var prediction = filtered + (trend * _prediction);

        // if it's too far - inhibit extrapolation
        diffVal = Vector3.Distance(prediction, sample);
        if (diffVal > maxDeviationRadius)
        {
            w = (maxDeviationRadius / diffVal);
            prediction = sample + w * (prediction - sample);
        }

        // ** Save the data from this frame
        this._history.Sample = sample;
        this._history.Filtered = filtered;
        this._history.Trend = trend;
        this._history.FrameCount++;

        // ** return the predicted data back into the joint
        return prediction;
    }

    // -------------------------------------------------------------------
    // -------------------------------------------------------------------
    private struct FilterDoubleExponentialData
    {
        public Vector3 Sample;
        public Vector3 Filtered;
        public Vector3 Trend;
        public uint FrameCount;
    }

    // -------------------------------------------------------------------
    // -------------------------------------------------------------------
    public class FilterDexpParams
    {
        private static float DEFAULT_SMOOTHING = 0.25f;
        private static float DEFAULT_CORRECTION = 0.25f;
        private static float DEFAULT_PREDICTION = 0.25f;
        private static float DEFAULT_JITTER_RADIUS = 0.03f; // = 3 cm.
        private static float DEFAULT_MAX_DEVIATION_RADIUS = 0.25f; // = 25 cm

        public float Smoothing { get; set; }
        public float Correction { get; set; }
        public float Prediction { get; set; }
        public float JitterRadius { get; set; }
        public float MaxDeviationRadius { get; set; }

        public FilterDexpParams()
        {
            Smoothing = DEFAULT_SMOOTHING;
            Correction = DEFAULT_CORRECTION;
            Prediction = DEFAULT_PREDICTION;
            JitterRadius = DEFAULT_JITTER_RADIUS;
            MaxDeviationRadius = DEFAULT_MAX_DEVIATION_RADIUS;
        }
    }
}