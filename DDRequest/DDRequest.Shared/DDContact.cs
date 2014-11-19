using System;
using System.Collections.Generic;
using System.Text;

namespace DDRequest
{
    using Windows.UI.Xaml.Media.Imaging;

    public class DDContact
    {
        public string Name { get; set; }
        public BitmapImage Thumbnail { get; set; }
        public string Phone { get; set; }
        public string Id { get; set; }
    }
}
