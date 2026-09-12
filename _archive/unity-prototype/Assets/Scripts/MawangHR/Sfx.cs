using System;
using UnityEngine;

namespace MawangHR
{
    /// 절차 생성 효과음 — 에셋 없이 코드로 파형 생성 (프로토 전용, 나중에 진짜 사운드로 교체).
    public static class Sfx
    {
        private static AudioSource src;
        private static AudioClip thunk, scratch, swish, pick, poof, shimmer, tok;

        public static void Init()
        {
            if (src != null) return;
            src = new GameObject("Sfx").AddComponent<AudioSource>();
            UnityEngine.Object.DontDestroyOnLoad(src.gameObject);

            var rng = new System.Random(666);
            Func<float> noise = () => (float)(rng.NextDouble() * 2.0 - 1.0);

            // 도장 쾅: 저음 둔탁 + 첫 순간 노이즈 타격
            thunk = Make("thunk", 0.28f, t =>
                (Mathf.Sin(2f * Mathf.PI * 72f * t) * 0.95f
                 + Mathf.Sin(2f * Mathf.PI * 141f * t) * 0.35f
                 + noise() * 0.6f * Mathf.Exp(-t * 80f))
                * Mathf.Exp(-t * 18f));

            // 마킹 사각: 짧은 노이즈 긁힘
            scratch = Make("scratch", 0.07f, t => noise() * 0.22f * Mathf.Exp(-t * 30f));

            // 서류 스윽: 부드러운 노이즈 스와이프
            swish = Make("swish", 0.2f, t => noise() * 0.13f * Mathf.Sin(Mathf.PI * t / 0.2f));

            // 도장 집기: 짧은 딸깍
            pick = Make("pick", 0.05f, t => Mathf.Sin(2f * Mathf.PI * 320f * t) * 0.25f * Mathf.Exp(-t * 90f));

            // 마법 소멸: 노이즈 + 내려가는 스윕
            poof = Make("poof", 0.3f, t =>
                (noise() * 0.4f + Mathf.Sin(2f * Mathf.PI * (400f - 700f * t) * t) * 0.3f)
                * Mathf.Exp(-t * 12f));

            // 물건 부딪힘: 짧은 톡
            tok = Make("tok", 0.09f, t =>
                (Mathf.Sin(2f * Mathf.PI * 190f * t) * 0.5f + noise() * 0.25f * Mathf.Exp(-t * 90f))
                * Mathf.Exp(-t * 45f));

            // 마법 재생성: 올라가는 반짝임 (3음 아르페지오)
            shimmer = Make("shimmer", 0.45f, t =>
            {
                float a = Mathf.Sin(2f * Mathf.PI * 660f * t) * Mathf.Exp(-t * 9f);
                float b = t > 0.08f ? Mathf.Sin(2f * Mathf.PI * 880f * (t - 0.08f)) * Mathf.Exp(-(t - 0.08f) * 9f) : 0f;
                float c = t > 0.16f ? Mathf.Sin(2f * Mathf.PI * 1320f * (t - 0.16f)) * Mathf.Exp(-(t - 0.16f) * 9f) : 0f;
                return (a + b + c) * 0.12f;
            });
        }

        public static void Thunk() => Play(thunk, 1f);
        public static void Scratch() => Play(scratch, 0.9f);
        public static void Swish() => Play(swish, 0.9f);
        public static void Pick() => Play(pick, 0.8f);
        public static void Poof() => Play(poof, 0.9f);
        public static void Shimmer() => Play(shimmer, 0.9f);
        public static void Tok(float vol) => Play(tok, Mathf.Clamp01(vol));

        private static void Play(AudioClip clip, float vol)
        {
            if (src != null && clip != null) src.PlayOneShot(clip, vol);
        }

        private static AudioClip Make(string name, float dur, Func<float, float> gen)
        {
            const int rate = 44100;
            int n = (int)(rate * dur);
            var data = new float[n];
            for (int i = 0; i < n; i++)
                data[i] = Mathf.Clamp(gen(i / (float)rate), -1f, 1f);
            var clip = AudioClip.Create(name, n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
