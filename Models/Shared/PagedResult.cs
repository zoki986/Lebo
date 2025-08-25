﻿namespace Lebo.Models.Shared
{
    public sealed class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public long TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }

}
