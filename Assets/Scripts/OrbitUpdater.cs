// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

namespace GalaxyExplorer
{
    public class OrbitUpdater : MonoBehaviour
    {
        /// <summary>
        /// Semi-major axis, km / DistanceScaleFactor
        /// </summary>
        public float SemiMajorAxis;
        public float SemiMajorAxisReal;

        public float CurrentSemiMajorAxis
        {
            get
            {
                return Mathf.Lerp(SemiMajorAxis, SemiMajorAxisReal * RealitySemiMajorAxisScale, Reality);
            }
        }

        /// <summary>
        /// Eccentricity
        /// </summary>
        public float Eccentricity;
        public float EccentricityReal;

        private float CurrentEccentricity
        {
            get
            {
                return Mathf.Lerp(Eccentricity, EccentricityReal, Reality);
            }
        }

        /// <summary>
        /// Argument of perigee, radians
        /// </summary>
        public float ArgumentOfPerigee;

        /// <summary>
        /// Inclination, radians
        /// </summary>
        public float Inclination;
        public float InclinationReal;

        private float CurrentInclination
        {
            get
            {
                return Mathf.Lerp(Inclination, InclinationReal, Reality);
            }
        }

        /// <summary>
        /// Right ascension of ascending node, radians
        /// </summary>
        public float RightAscensionOfAscendingNode;

        /// <summary>
        /// Mean anomaly, radians
        /// </summary>
        public float MeanAnomaly;

        /// <summary>
        /// Sidereal period, days
        /// </summary>
        public float Period;
        public float PeriodReal;

        public float CurrentPeriod
        {
            get
            {
                return Mathf.Lerp(Period, PeriodReal, Reality);
            }
        }

        /// <summary>
        /// Orbital plane
        /// </summary>
        public Coordinates Plane;

        /// <summary>
        /// True anomaly, radians
        /// </summary>
        public float TrueAnomaly;

        /// <summary>
        /// Epoch, JD
        /// </summary>
        public float Epoch;

        /// <summary>
        /// Realilty from 0 (schematic) to 1 (real)
        /// </summary>
        [Range(0, 1)]
        public float Reality;

        public float RealitySemiMajorAxisScale = 1;

        public float Speed = 100.0f;
        public float SpeedMultiplier = 1;
        public float TransitionSpeedMultiplier = 1.0f;

        public int MaxIterations = 50;

        private DateTime myDateTime;

        public DateTime StartDate { get; private set; }

        private bool computed = false;

        // Use this for initialization
        private void Start()
        {
            StartDate = myDateTime = DateTime.Now;
        }

        // Update is called once per frame
        private void Update()
        {
            if (computed && TransitionSpeedMultiplier == 0.0f)
            {
                // Don't animate the planet rotation during transitions
                return;
            }

            myDateTime += TimeSpan.FromDays(Time.deltaTime * Speed * SpeedMultiplier * TransitionSpeedMultiplier);
            transform.localPosition = CalculatePosition(myDateTime);

            computed = true;
        }

        public Vector3 CalculatePosition(DateTime time)
        {
            float trueAnomoly = CalculateTrueAnomaly(time);
            return CalculatePosition(trueAnomoly);
        }

        /// <summary>
        /// True anomaly using Newton-Raphson iteration
        /// </summary>
        /// <param name="orbit"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public float CalculateTrueAnomaly(DateTime dateTime)
        {
            const float epsilon = 0.000001f;
            float trueAnomoly;

            // Mean Anomoly
            float meanAnomoly = CalculateMeanAnomaly(dateTime);

            // Eccentric Anomaly
            float eccentricAnomoly, oldEccentricAnomoly, newEccentricAnomoly = meanAnomoly + (CurrentEccentricity / 2);

            // Solve [ 0 = E - e sin(E) - M ] for E using Newton-Raphson: Xn+1 = Xn - [ f(Xn) / f'(Xn) ]
            // E = Eccentric Anomaly, M = Mean Anomaly
            int iterations = 0;
            do
            {
                iterations++;
                oldEccentricAnomoly = newEccentricAnomoly;
                newEccentricAnomoly = oldEccentricAnomoly - (oldEccentricAnomoly - (CurrentEccentricity * Mathf.Sin(oldEccentricAnomoly)) - meanAnomoly) / (1.0f - (CurrentEccentricity * Mathf.Cos(oldEccentricAnomoly)));
            }
            while (Mathf.Abs(oldEccentricAnomoly - newEccentricAnomoly) > epsilon && iterations < MaxIterations);

            // Iteractions
            if (iterations == MaxIterations)
            {
                trueAnomoly = TrueAnomaly;
            }
            else
            {
                eccentricAnomoly = newEccentricAnomoly;
                float cosEccentricAnomoly = Mathf.Cos(eccentricAnomoly);

                // Solve cos(bodyAngle) = ( cos(E) - e ) / (1 - e cos(E) ) to get body angle with sun
                // E = Eccentric Anomaly, e = Eccentricity
                trueAnomoly = Mathf.Acos((cosEccentricAnomoly - CurrentEccentricity) / (1.0f - (CurrentEccentricity * cosEccentricAnomoly)));

                // Arccos returns value between 0 and Pi, but when Mean Anomoly > Pi (ie past halfway point) take (TwoPi - angle) to get solution between Pi and TwoPi
                if (meanAnomoly > Mathf.PI)
                {
                    trueAnomoly = (Mathf.PI * 2.0f) - trueAnomoly;
                }

                // Fail
                if (float.IsNaN(trueAnomoly))
                {
                    trueAnomoly = TrueAnomaly;
                }
            }

            return trueAnomoly;
        }

        /// <summary>
        /// Mean anomaly
        /// </summary>
        /// <returns>Mean anomaly, radians (0 to TwoPi)</returns>
        public float CalculateMeanAnomaly(DateTime dateTime)
        {
            float angle = (ElapsedDays(dateTime, Epoch) % CurrentPeriod) / CurrentPeriod * (Mathf.PI * 2) * (-1);

            // Add mean anomaly at defined epoch
            angle += MeanAnomaly;

            // Wrap angle 0-TwoPi
            if (angle > Mathf.PI * 2)
            {
                angle -= Mathf.PI * 2;
            }
            else if (angle < 0)
            {
                angle += Mathf.PI * 2;
            }

            return angle;
        }

        /// <summary>
        /// Calculate position in orbit relative to orbit origin
        /// </summary>
        /// <param name="trueAnomaly">True anomaly, radians</param>
        public Vector3 CalculatePosition(float trueAnomaly)
        {
            // Compute radius from orbit origin
            float radius = CurrentSemiMajorAxis * (1 - CurrentEccentricity * CurrentEccentricity) / (1 + CurrentEccentricity * Mathf.Cos(trueAnomaly));

            // Calculate position relative to orbit origin
            // XZ-plane is ecliptic, Y towards celestial north pole
            return new Vector3(
                radius * (Mathf.Cos(RightAscensionOfAscendingNode) * Mathf.Cos(trueAnomaly + ArgumentOfPerigee) - Mathf.Sin(RightAscensionOfAscendingNode) * Mathf.Sin(trueAnomaly + ArgumentOfPerigee) * Mathf.Cos(CurrentInclination)),
                radius * (Mathf.Sin(trueAnomaly + ArgumentOfPerigee) * Mathf.Sin(CurrentInclination)),
                -radius * (Mathf.Sin(RightAscensionOfAscendingNode) * Mathf.Cos(trueAnomaly + ArgumentOfPerigee) + Mathf.Cos(RightAscensionOfAscendingNode) * Mathf.Sin(trueAnomaly + ArgumentOfPerigee) * Mathf.Cos(CurrentInclination)));
        }

        /// <summary>
        /// Elapsed days since J2000.0
        /// </summary>
        /// <returns></returns>
        public float ElapsedDays(DateTime dateTime, float epoch)
        {
            return ToJulianDate(dateTime) - epoch;
        }

        public float ToJulianDate(DateTime dateTime)
        {
            return ToJulianDay(dateTime) + (dateTime.Hour - 12) / 24f + dateTime.Minute / 1440f + dateTime.Second / 86400f;
        }

        public long ToJulianDay(DateTime dateTime)
        {
            int month = dateTime.Month;
            int day = dateTime.Day;
            int year = dateTime.Year;
            if (month < 3)
            {
                month += 12;
                year -= 1;
            }

            return day + (153 * month - 457) / 5 + 365 * year + (year / 4) - (year / 100) + (year / 400) + 1721119;
        }

        public enum Coordinates
        {
            Ecliptic,
            Equatorial,
            Galactic,
            Laplace
        }
    }
}