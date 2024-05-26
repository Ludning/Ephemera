using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyelessDogHealth : LivingEntity
{
    [SerializeField] EyelessDogAI eyelessDog;

    private void OnEnable()
    {
        maxHealth = 120f;
        health = maxHealth;
        dead = false;
    }

    public override bool ApplyDamage(DamageMessage damageMessage)
    {
        //데미지 주는 것이 자기 자신이거나, 자신이 죽었으면 실패.
        if (!base.ApplyDamage(damageMessage)) return false;

        Debug.Log("눈없는 개" + damageMessage.damage + " 피해입음");

        //눈없는 개는 피해를 입든말든 소리난 쪽으로 반응
        
        return true;
    }

    public override void Die()
    {
        base.Die();
        Debug.Log("눈없는 개 죽음");
    }
}
