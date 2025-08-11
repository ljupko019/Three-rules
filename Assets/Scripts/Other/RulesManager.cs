using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RulesManager : MonoBehaviour
{
    public enum NegativeRule
    {
        NoHealing,
        MeleeOnly,
        OneHitDeath,
        TimeLimit,
        SlowerMovement,
        NoDash,
        Vulnerable,
    }

    public enum PositiveRule
    {
        DoubleDamage,
        VampireTouch,
        SpeedIncrease,
        InfiniteAmmo
    }

    public List<NegativeRule> activeNegativeRules = new List<NegativeRule>();
    public List<PositiveRule> activePositiveRules = new List<PositiveRule>();
    PlayerController player;
    public TMP_Text postiviRulesText;
    public TMP_Text negativeRulesText;

    public GameObject panel1;
    public GameObject panel2;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        ResetRules();
        SetRules();
        ShowRulesOnScreen();
    }
    public void AddNegativeRule(NegativeRule rule)
    {
        if (!activeNegativeRules.Contains(rule))
            activeNegativeRules.Add(rule);
    }

    public void AddPositiveRule(PositiveRule rule)
    {
        if (!activePositiveRules.Contains(rule))
            activePositiveRules.Add(rule);
    }

    public void ClearAllRules()
    {
        activeNegativeRules.Clear();
        activePositiveRules.Clear();
    }

   public void SetRules()
    {
        ClearAllRules();
        
        var negativeValues = System.Enum.GetValues(typeof(NegativeRule));
        var positiveValues = System.Enum.GetValues(typeof(PositiveRule));

     
        while (activeNegativeRules.Count < 2)
        {
            NegativeRule randomNeg = (NegativeRule)negativeValues.GetValue(Random.Range(0, negativeValues.Length));
            if (!activeNegativeRules.Contains(randomNeg))
                activeNegativeRules.Add(randomNeg);
        }

        PositiveRule randomPos = (PositiveRule)positiveValues.GetValue(Random.Range(0, positiveValues.Length));
        activePositiveRules.Add(randomPos);

        Debug.Log(activePositiveRules[0]);
        Debug.Log(activeNegativeRules[0] + " " + activeNegativeRules[1]);

        ApplyRules();
    }
    private void ShowRulesOnScreen()
    {
        postiviRulesText.color = Color.green;
        negativeRulesText.color = Color.red;
        string posDisplay = "<b>Positive rule:</b>\n";
        foreach (var posRule in activePositiveRules)
        {
            posDisplay += "- " + posRule.ToString() + "\n";
        }
        postiviRulesText.text = posDisplay;

        // Negativna pravila
        string negDisplay = "<b>Negative rules:</b>\n";
        foreach (var negRule in activeNegativeRules)
        {
            negDisplay += "- " + negRule.ToString() + "\n";
        }
        negativeRulesText.text = negDisplay;

        StartCoroutine(ClearRulesText(3.5f));
    }
    private void DoubleDamage() 
    {
        player.SetDoubleDamage();
    }
    private void VampireTouch() 
    {
        player.SetVampireTouch(true);
    }
    private void SpeedIncresed() 
    {
        player.SetSpeedIncrease();
    }
    private void InfiniteAmmo() 
    {
        player.SetInfiniteAmmo(true);
    }

    private void UnsetDoubleDamage()
    {
        player.UnsetDoubleDamage();
    }
    private void UnsetVampireTouch()
    {
        player.SetVampireTouch(false);
    }
    private void UnsetInfiniteAmmo()
    {
        player.SetInfiniteAmmo(false);
    }
    private void NoHealing() 
    {
        player.SetCanHeal(false);
    }

    private void MeleeOnly() 
    {
        player.SetCanUseGun(false);
    }

    private void OneHitDeath() 
    {
        player.SetOneHitDeath(true);
    }

    private void TimeLimit() 
    {
        player.SetIsTimeLimited(true);
    }

    private void SlowerMovement() 
    {
        player.SetSlowerSpeed();
    }

    private void NoDash() 
    {
        player.SetOffDash(false);
    }

    private void Vulnerable()
    {
        player.SetVulnerable(true);
    }

    private void UnsetNoHealing()
    {
        player.SetCanHeal(true);
    }

    private void UnsetMeleeOnly()
    {
        player.SetCanUseGun(true);
    }

    private void UnsetOneHitDeath()
    {
        player.SetOneHitDeath(false);
    }

    private void UnsetTimeLimit()
    {
        player.SetIsTimeLimited(false);
    }

    private void UnslowerSlowerMovement()
    {
        player.SetNormalSpeed();
    }

    private void UnsetNoDash()
    {
        player.SetOffDash(true);
    }

    private void UnsetVulnerable()
    {
        player.SetVulnerable(false);
    }

    private void ResetRules() 
    {
        UnsetMeleeOnly();
        UnsetNoDash();
        UnsetNoHealing();
        UnsetOneHitDeath();
        UnsetTimeLimit();
        UnsetVulnerable();
        UnslowerSlowerMovement();
        UnsetDoubleDamage();
        UnsetVampireTouch();
        UnsetInfiniteAmmo();
    }

    public void ApplyRules()
    {
        foreach (var posRule in activePositiveRules)
        {
            switch (posRule)
            {
                case PositiveRule.DoubleDamage:
                    DoubleDamage();
                    break;
                case PositiveRule.VampireTouch:
                    VampireTouch();
                    break;
                case PositiveRule.SpeedIncrease:
                    SpeedIncresed();
                    break;
                case PositiveRule.InfiniteAmmo:
                    InfiniteAmmo();
                    break;
            }
        }

        foreach (var negRule in activeNegativeRules)
        {
            switch (negRule)
            {
                case NegativeRule.NoHealing:
                    NoHealing();
                    break;
                case NegativeRule.MeleeOnly:
                    MeleeOnly();
                    break;
                case NegativeRule.OneHitDeath:
                    OneHitDeath();
                    break;
                case NegativeRule.TimeLimit:
                    TimeLimit();
                    break;
                case NegativeRule.SlowerMovement:
                    SlowerMovement();
                    break;
                case NegativeRule.NoDash:
                    NoDash();
                    break;
                case NegativeRule.Vulnerable:
                    Vulnerable();
                    break;
            }
        }
    }

    private IEnumerator ClearRulesText(float delay)
    {
        yield return new WaitForSeconds(delay);
        postiviRulesText.text = "";
        negativeRulesText.text = "";

        panel1.SetActive(false);
        panel2.SetActive(false);
    }
}

