using System;
using UnityEngine;

namespace MawangHR
{
    /// 절차 생성 효과음 — 에셋 없이 코드로 파형 생성 (프로토 전용, 나중에 진짜 사운드로 교체).
    public static class Sfx
    {
        private static AudioSource src;
        private static AudioClip thunk, scratch, swish, pick;

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
        }

        public static void Thunk() => Play(thunk, 1f);
        public static void Scratch() => Play(scratch, 0.9f);
        public static void Swish() => Play(swish, 0.9f);
        public static void Pick() => Play(pick, 0.8f);

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
