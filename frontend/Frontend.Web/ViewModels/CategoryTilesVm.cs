namespace Frontend.Web.ViewModels;

public class CategoryTilesVm
{
    public string Cat { get; }
    public IReadOnlyList<string> Pics { get; }
    public IReadOnlyList<Guid> ItemIds { get; }

    public CategoryTilesVm(string cat, IEnumerable<string> pics, IEnumerable<Guid> itemIds)
    {
        Cat = cat;
        Pics = pics.ToList();
        ItemIds = itemIds.ToList();
    }
}
