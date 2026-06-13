using UnityEngine;

namespace shwngFpsCameraStances
{
    public static class StanceTransitionCurves
    {
        public static AnimationCurve PitchXCurve;
        public static AnimationCurve YawYCurve;
        public static AnimationCurve RollZCurve;
        public static AnimationCurve PosZCurve;
        public static AnimationCurve PosXCurve;

        private static bool _initialized = false;

        public static void Initialize()
        {
            if (_initialized) return;

            // Pitch X (Cano subindo/descendo)
            PitchXCurve = new AnimationCurve(
                new Keyframe(0.0f, 0f),
                new Keyframe(0.35f, 2.5f),
                new Keyframe(0.75f, -1.5f),
                new Keyframe(1.0f, 0f)
            );

            // Yaw Y (Tombo da luneta/arma)
            YawYCurve = new AnimationCurve(
                new Keyframe(0.0f, 0f),
                new Keyframe(0.40f, 3.0f),
                new Keyframe(0.80f, -1.0f),
                new Keyframe(1.0f, 0f)
            );

            // Roll Z (Apontar Esquerda/Direita)
            RollZCurve = new AnimationCurve(
                new Keyframe(0.0f, 0f),
                new Keyframe(0.30f, -1.5f),
                new Keyframe(0.70f, 0.8f),
                new Keyframe(1.0f, 0f)
            );

            // Forward/Backward Z (Coronha movendo verticalmente na transição)
            // Nota: O eixo local Forward na Unity e Tarkov geralmente é Z.
            PosZCurve = new AnimationCurve(
                new Keyframe(0.0f, 0f),
                new Keyframe(0.40f, 0.02f),
                new Keyframe(0.85f, -0.015f),
                new Keyframe(1.0f, 0f)
            );

            // Sideways X (Coronha afastando/aproximando do corpo)
            PosXCurve = new AnimationCurve(
                new Keyframe(0.0f, 0f),
                new Keyframe(0.35f, 0.01f),
                new Keyframe(0.75f, -0.01f),
                new Keyframe(1.0f, 0f)
            );

            // Para garantir que as curvas sejam interpoladas de forma suave (Splines),
            // aplicamos o modo Auto para todas as chaves.
            SmoothCurve(PitchXCurve);
            SmoothCurve(YawYCurve);
            SmoothCurve(RollZCurve);
            SmoothCurve(PosZCurve);
            SmoothCurve(PosXCurve);

            _initialized = true;
        }

        private static void SmoothCurve(AnimationCurve curve)
        {
            for (int i = 0; i < curve.length; i++)
            {
                curve.SmoothTangents(i, 0f);
            }
        }

        public static Vector3 EvaluateRotation(float progress)
        {
            if (!_initialized) Initialize();
            // Vector3 rotações em graus
            return new Vector3(
                PitchXCurve.Evaluate(progress),
                YawYCurve.Evaluate(progress),
                RollZCurve.Evaluate(progress)
            );
        }

        public static Vector3 EvaluatePosition(float progress)
        {
            if (!_initialized) Initialize();
            // Vetor de translação local
            return new Vector3(
                PosXCurve.Evaluate(progress),
                0f, // Sem curva Y por enquanto
                PosZCurve.Evaluate(progress)
            );
        }
    }
}
