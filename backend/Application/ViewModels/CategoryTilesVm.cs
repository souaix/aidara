namespace Frontend.Web.ViewModels;

public class CategoryTilesVm
{
    public string Cat { get; }
    public List<string> Pics { get; }
    public List<Guid> ItemIds { get; }   // 對應服務項目的 item_id

    public CategoryTilesVm(string cat, IEnumerable<string> pics, IEnumerable<Guid> itemIds)
    {
        Cat = cat;
        Pics = pics.ToList();
        ItemIds = itemIds.ToList();
    }
}
