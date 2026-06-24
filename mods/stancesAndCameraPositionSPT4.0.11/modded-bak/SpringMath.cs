using UnityEngine;

namespace CameraRotationMod
{
    public static class SpringMath
    {
        /// <summary>
        /// Mathematically correct Damped Harmonic Oscillator for Vector3.
        /// </summary>
        /// <param name="current">Current position/rotation</param>
        /// <param name="target">Target position/rotation</param>
        /// <param name="velocity">Current velocity (modified by reference)</param>
        /// <param name="dampingRatio">1.0 = Critically damped (SmoothDamp), < 1.0 = Under-damped (Bounce/Overshoot), > 1.0 = Over-damped</param>
        /// <param name="angularFrequency">Speed of the spring (higher = faster)</param>
        /// <param name="deltaTime">Time step</param>
        public static Vector3 SpringDamp(Vector3 current, Vector3 target, ref Vector3 velocity, float dampingRatio, float angularFrequency, float deltaTime)
        {
            Vector3 delta = current - target;
            
            if (angularFrequency < 0.0001f) return current;

            float expTerm = Mathf.Exp(-dampingRatio * angularFrequency * deltaTime);

            if (dampingRatio < 1.0f) // Under-damped (Bouncy)
            {
                float omegaD = angularFrequency * Mathf.Sqrt(1.0f - dampingRatio * dampingRatio);
                float cosD = Mathf.Cos(omegaD * deltaTime);
                float sinD = Mathf.Sin(omegaD * deltaTime);

                Vector3 c2 = (velocity + dampingRatio * angularFrequency * delta) / omegaD;
                
                current = target + expTerm * (delta * cosD + c2 * sinD);
                velocity = -expTerm * ((dampingRatio * angularFrequency * delta - omegaD * c2) * cosD + (omegaD * delta + dampingRatio * angularFrequency * c2) * sinD);
            }
            else if (Mathf.Approximately(dampingRatio, 1.0f)) // Critically damped (Like SmoothDamp)
            {
                Vector3 c2 = velocity + angularFrequency * delta;
                current = target + (delta + c2 * deltaTime) * expTerm;
                velocity = (velocity - c2 * (angularFrequency * deltaTime)) * expTerm;
            }
            else // Over-damped
            {
                float omegaD = angularFrequency * Mathf.Sqrt(dampingRatio * dampingRatio - 1.0f);
                float r1 = -angularFrequency * dampingRatio + omegaD;
                float r2 = -angularFrequency * dampingRatio - omegaD;
                
                Vector3 c2 = (velocity - r1 * delta) / (r2 - r1);
                Vector3 c1 = delta - c2;
                
                float e1 = Mathf.Exp(r1 * deltaTime);
                float e2 = Mathf.Exp(r2 * deltaTime);
                
                current = target + c1 * e1 + c2 * e2;
                velocity = c1 * r1 * e1 + c2 * r2 * e2;
            }

            return current;
        }

        /// <summary>
        /// Converts SmoothDamp's smoothTime parameter roughly into an angularFrequency for SpringDamp.
        /// SmoothTime is approximately the time taken to reach ~63% of the target.
        /// </summary>
        public static float SmoothTimeToAngularFrequency(float smoothTime)
        {
            if (smoothTime <= 0.0001f) return 10000f; // Instant
            return 2.0f / smoothTime; // Standard mapping for critical damping
        }
    }
}
