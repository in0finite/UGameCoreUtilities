﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UGameCore.Utilities
{

	public class DamageInfo
	{
		public float amount = 0f;
		public string damageType = null;
		public Transform raycastHitTransform = null;
		public Vector3 hitDirection = Vector3.forward;
		public Vector3 hitPoint = Vector3.zero;
		public Vector3 hitNormal = Vector3.up;
		public object attacker = null;
		public object attackingPlayer = null;
		public object data = null;
	}

	public static class DamageType
	{
		public static readonly string
			Bullet = "Bullet",
			Explosion = "Explosion",
			Gas = "Gas",
			Flame = "Flame",
			Melee = "Melee";
	}

	public class Damageable : MonoBehaviour
	{

		[SerializeField] private float m_health = 0f;
		public float Health { get { return m_health; } set { m_health = value; } }

		[SerializeField] private UnityEvent m_onDamage = new UnityEvent ();
		public UnityEvent OnDamageEvent => m_onDamage;

		public DamageInfo LastDamageInfo { get; private set; }



		public void Damage (DamageInfo info)
		{
			this.LastDamageInfo = info;

			F.RunExceptionSafe(() => m_onDamage.Invoke());
		}

		public void HandleDamageByDefault ()
		{
			DamageInfo info = this.LastDamageInfo;

			this.Health -= info.amount;

			if (this.Health <= 0f) {
				Destroy (this.gameObject);
			}
		}

		public static void InflictDamageToObjectsInArea(
			Vector3 center,
			float radius,
			float damageAmount,
			AnimationCurve damageOverDistanceCurve,
			string damageType,
			object attacker = null,
			object attackingPlayer = null)
		{
			Collider[] overlappingColliders = Physics.OverlapSphere(center, radius);

			var damagables = new Dictionary<Damageable, List<Collider>>();

			foreach (var collider in overlappingColliders)
			{
				var damagable = collider.GetComponentInParent<Damageable>();
				if (damagable != null)
				{
					if (damagables.ContainsKey(damagable))
					{
						damagables[damagable].Add(collider);
					}
					else
					{
						damagables.Add(damagable, new List<Collider>() { collider });
					}
				}
			}

			foreach (var pair in damagables)
			{
				Damageable damageable = pair.Key;
				List<Collider> colliders = pair.Value;

				// find closest point from all colliders

				float closestPointDistance = float.MaxValue;

				foreach (var collider in colliders)
				{
					Vector3 closestPointOnCollider = collider.ClosestPointOrBoundsCenter(center);
					float distanceToPointOnCollider = Vector3.Distance(center, closestPointOnCollider);

					if (distanceToPointOnCollider < closestPointDistance)
					{
						closestPointDistance = distanceToPointOnCollider;
					}

				}

				// apply damage based on closest distance

				float distance = closestPointDistance;
				float distanceFactor = damageOverDistanceCurve.Evaluate(Mathf.Clamp01(distance / radius));
				float damageAmountBasedOnDistance = damageAmount * distanceFactor;

				F.RunExceptionSafe(() => damageable.Damage(new DamageInfo
				{
					amount = damageAmountBasedOnDistance,
					damageType = damageType,
					attacker = attacker,
					attackingPlayer = attackingPlayer,
				}));
			}

		}

	}

}
