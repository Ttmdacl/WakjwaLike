using UnityEngine;

[CreateAssetMenu(fileName = "EffectLootItem", menuName = "Effect/EffectLootItem")]
public class EffectLootItem : Effect
{
    [SerializeField] private GameEvent OnMonsterCollapse;
    [SerializeField] private Item item;

    private void DropItem(Transform transform) => ObjectManager.Instance.PopObject("ItemGameObject", transform.position).GetComponent<ItemGameObject>().Initialize(item.id);

    public override void _Effect() => OnMonsterCollapse.AddCollback(DropItem);

    public override void Return() => OnMonsterCollapse.RemoveCollback(DropItem);
}
