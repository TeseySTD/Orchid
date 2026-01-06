namespace Orchid.Core.Models;

public class Navigation
{
    public List<NavItem> NavItems { get; init; }

    private Navigation(IEnumerable<NavItem> navItems)
    {
        NavItems = navItems.ToList();
    }

    public static Navigation Create(IEnumerable<NavItem> navItems) => new(navItems);

    public int Count => GetRecursiveCount(NavItems);

    private int GetRecursiveCount(List<NavItem> navItems)
    {
        int totalNavItemCount = navItems.Count();

        foreach (var item in navItems)
            if (item.NestedItems?.Any() ?? false)
                totalNavItemCount += GetRecursiveCount(item.NestedItems);

        return totalNavItemCount;
    }

}