using static UnityEngine.ParticleSystem;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class SpriteExplosion : MonoBehaviour
{
    [SerializeField] private float _maxSimulationSpeed;
    [SerializeField] private AnimationCurve _simulationSpeedCurve;
    [SerializeField] private float _maxSpeedTime;

    [SerializeField] private float _particleSize;
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    public void ExplodeSprite(Vector3 position, Sprite sprite)
    {
        Vector2 spriteDimension = sprite.rect.size;
        List<ParticleSystem.Particle> particleList = new();

        var pixels = sprite.texture.GetPixels();
        var offset = sprite.pivot * _particleSize;

        var main = _particleSystem.main;
        main.simulationSpeed = 0f;

        for (int x = 0; x < spriteDimension.x; x++)
        {
            for (int y = 0; y < spriteDimension.y; y++)
            {
                Color pixelColor = sprite.texture.GetPixel(x + (int)sprite.rect.min.x, y + (int)sprite.rect.min.y);
                if (pixelColor.a != 0f)
                {
                    var p = new ParticleSystem.Particle();
                    p.startSize = _particleSize;
                    p.position = position + new Vector3((0.5f + x) * _particleSize - offset.x, (0.5f + y) * _particleSize - offset.y);
                    p.startLifetime = Random.Range(8f, 12f);
                    p.remainingLifetime = p.startLifetime;
                    p.startColor = pixelColor;

                    var dir = (p.position - position);
                    var force = Random.Range(3f, 6f);

                    p.velocity = force * dir;

                    particleList.Add(p);
                }
            }
        }

        _particleSystem.SetParticles(particleList.ToArray());

        PlayerCamera.Instance.PlayerDeathShaker.PlayFeedbacks();

        StartCoroutine(SimulationCo(main));
    }

    private IEnumerator SimulationCo(MainModule main)
    {
        var t = 0f;
        var simulationSpeed = 0f;
        while (t < _maxSpeedTime)
        {
            t += Time.deltaTime;

            var curve = _simulationSpeedCurve.Evaluate(t / _maxSpeedTime);
            simulationSpeed = curve * _maxSimulationSpeed;
            main.simulationSpeed = simulationSpeed;

            yield return null;
        }
    }
}