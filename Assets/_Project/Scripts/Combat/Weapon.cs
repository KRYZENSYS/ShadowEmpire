// =============================================================================
// Weapon.cs
// =============================================================================
// PURPOSE:
//   Realistic weapon system: recoil, ballistics, fire rate, ammo, reload.
//   Attach to weapon root GameObject. Hooks: Muzzle, Eject, Mag, Grip.
//
// ENGINE: Unity 6
// =============================================================================

using UnityEngine;
using ShadowEmpire.AI;

namespace ShadowEmpire.Combat
{
    public class Weapon : MonoBehaviour
    {
        public enum FireMode { Semi, Auto, Burst }
        public enum WeaponType { Pistol, Rifle, SMG, Sniper, Shotgun }

        [Header("Identity")]
        public string weaponName = "AK-47";
        public WeaponType type = WeaponType.Rifle;
        public FireMode fireMode = FireMode.Auto;

        [Header("Stats")]
        public float damage = 30f;
        public float range = 80f;
        public float fireRate = 0.09f;     // seconds between shots
        public int magazineSize = 30;
        public float reloadTime = 2.4f;
        public float accuracy = 0.95f;     // 1.0 = perfect

        [Header("Ballistics")]
        public float recoilKick = 0.04f;
        public float recoilUp = 1.8f;
        public float recoilSide = 0.6f;
        public float bulletDrop = 0.005f;
        public int pellets = 1;            // shotgun pellets

        [Header("FX Hooks")]
        public Transform muzzle;
        public ParticleSystem muzzleFlash;
        public AudioSource audioSrc;
        public AudioClip fireClip;
        public AudioClip reloadClip;
        public TrailRenderer bulletTrailPrefab;

        private int _ammo;
        private float _nextFireTime;
        private bool _isReloading;
        private Camera _cam;

        public int Ammo => _ammo;
        public bool IsReloading => _isReloading;

        private void Awake()
        {
            _ammo = magazineSize;
            _cam = Camera.main;
        }

        public void TryFire()
        {
            if (_isReloading) return;
            if (Time.time < _nextFireTime) return;
            if (_ammo <= 0) { Reload(); return; }

            _nextFireTime = Time.time + fireRate;
            Fire();
        }

        private void Fire()
        {
            _ammo--;
            // Apply recoil
            Player.PlayerController.Instance?.AddRecoil(recoilKick, recoilUp, recoilSide);

            // Fire bullets
            for (int i = 0; i < pellets; i++)
            {
                Vector3 dir = _cam.transform.forward;
                Vector3 spread = Random.insideUnitSphere * (1f - accuracy) * 0.05f;
                dir += spread;

                if (Physics.Raycast(_cam.transform.position, dir, out var hit, range))
                {
                    // Damage
                    var dmg = hit.collider.GetComponentInParent<IDamageable>();
                    if (dmg != null) dmg.TakeDamage(damage * (type == WeaponType.Sniper ? 2.5f : 1f));

                    // Impact VFX hook
                    SpawnImpact(hit.point, hit.normal);
                }
                SpawnTrail(muzzle.position, hit.point);
            }

            // FX
            muzzleFlash?.Play();
            audioSrc?.PlayOneShot(fireClip);

            if (_ammo <= 0) Reload();
        }

        public void Reload()
        {
            if (_isReloading || _ammo == magazineSize) return;
            StartCoroutine(ReloadRoutine());
        }

        private System.Collections.IEnumerator ReloadRoutine()
        {
            _isReloading = true;
            audioSrc?.PlayOneShot(reloadClip);
            yield return new WaitForSeconds(reloadTime);
            _ammo = magazineSize;
            _isReloading = false;
        }

        // -------- VFX --------
        private void SpawnTrail(Vector3 from, Vector3 to)
        {
            if (bulletTrailPrefab == null) return;
            var t = Instantiate(bulletTrailPrefab, from, Quaternion.identity);
            t.AddPosition(from);
            t.AddPosition(to);
            Destroy(t.gameObject, 0.1f);
        }
        private void SpawnImpact(Vector3 pos, Vector3 normal)
        {
            // Hook your impact prefab pool here
        }
    }
}