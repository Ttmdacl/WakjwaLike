using System.Linq;
using UnityEngine;
using FMODUnity;

public class ShopKeeper : NPC
{
    [SerializeField] private ItemInventoryUI itemInventoryUI_Buy;
    [SerializeField] private IntVariable nyang;

    protected override void Awake()
    {
        base.Awake();

        for (int i = 0; i < 5; i++)
            itemInventoryUI_Buy.NpcInventory.Add(DataManager.Instance.ItemDic.ElementAt(Random.Range(0, DataManager.Instance.ItemDic.Count)).Value);
    }

    public void BuyItem(Slot slot)
    {
        if (nyang.RuntimeValue >= (slot.SpecialThing as Item).price)
        {
            nyang.RuntimeValue -= (slot.SpecialThing as Item).price;

            slot.gameObject.SetActive(false);

            itemInventoryUI_Buy.NpcInventory.Remove(slot.SpecialThing as Item);
            ObjectManager.Instance.PopObject("ItemGameObject", transform).GetComponent<ItemGameObject>().Initialize((slot.SpecialThing as Item).id);
            RuntimeManager.PlayOneShot($"event:/SFX/UI/Test", transform.position);
            itemInventoryUI_Buy.Initialize();
        }
        else
        {
            RuntimeManager.PlayOneShot($"event:/SFX/UI/No", transform.position);
            ObjectManager.Instance.PopObject("AnimatedText", Wakgood.Instance.transform.position).GetComponent<AnimatedText>().SetText("골두 부족!", Color.yellow);
        }
    }
}
