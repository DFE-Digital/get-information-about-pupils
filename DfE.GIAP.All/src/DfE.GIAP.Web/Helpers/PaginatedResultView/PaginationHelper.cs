namespace DfE.GIAP.Web.Helpers.PaginatedResultView;

public static class PaginationHelper
{
    public static IEnumerable<PaginationItem> Build(int current, bool morePages)
    {
        //
        // 1. Always show page 1
        //
        yield return new PaginationItem(1, IsCurrent: current == 1);

        //
        // CASE: current = 1
        //
        if (current == 1)
        {
            if (morePages)
            {
                // Show page 2
                yield return new PaginationItem(2);

                // DO NOT show ellipsis on page 1
                // because we cannot know if page 3 exists yet
            }

            yield break;
        }

        //
        // 2. Determine sliding window for pages beyond page 1
        //
        int start;
        int end;

        if (morePages)
        {
            start = Math.Max(2, current - 1);
            end = current + 1;
        }
        else
        {
            end = current;
            start = Math.Max(2, end - 2);
        }

        //
        // 3. Ellipsis after page 1 if needed
        //
        if (start > 2)
            yield return PaginationItem.Ellipsis();

        //
        // 4. Main window
        //
        for (var page = start; page <= end; page++)
        {
            yield return new PaginationItem(page, IsCurrent: page == current);
        }

        //
        // 5. Ellipsis at the end if more pages exist
        //
        if (morePages)
            yield return PaginationItem.Ellipsis();
    }
}


public record PaginationItem(int? Page, bool IsCurrent = false, bool IsEllipsis = false)
{
    public static PaginationItem Ellipsis() => new PaginationItem(null, false, true);
}

