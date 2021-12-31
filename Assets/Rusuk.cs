using UnityEngine;
using System.Collections;

public class Rusuk : Chef
{
    private Slot tempSlot;
    public override void BuyFood(Slot slot)
    {
        tempSlot = slot;
        canOpenUI = false;
        customUI.SetActive(false);
        StartCoroutine(nameof(Shaking));
    }

    private IEnumerator Shaking()
    {
        cvm1.Priority = 200;
        cvm2.Priority = -100;
        GameManager.Instance.CinemachineTargetGroup.m_Targets[1].target = transform;
        GameManager.Instance.CinemachineTargetGroup.m_Targets[1].weight = 5;
        animator.SetBool("SAKING", true);
        yield return new WaitForSeconds(1.5f);
        animator.SetBool("SAKING", false);
        wakgoodFoodInventory.Add(tempSlot.SpecialThing as Food);
        DataManager.Instance.wgItemInven.Add(DataManager.Instance.ItemDic[57]);
        base.FocusOff();
    }
}
