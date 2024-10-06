﻿namespace NimbusProto2.YADISK
{
#pragma warning disable IDE1006 // Naming Styles
    public class ResourcesRoot
    {
        public ResourceList? _embedded { get; set; }
    }

    public class ResourceList
    {
        public ResourcesItem[]? items { get; set; }
    }

    public class ResourcesItem
    {
        public string? name { get; set; }
        public string? resource_id { get; set; }
        public DateTime created { get; set; }
        public DateTime modified { get; set; }
        public string? public_key { get; set; }
        public string? public_url { get; set; }
        public string? path { get; set; }
        public string? type { get; set; }
        public string? preview { get; set; }
        public string? mime_type { get; set; }
        public long size { get; set; }
    }
#pragma warning restore IDE1006 // Naming Styles

}
