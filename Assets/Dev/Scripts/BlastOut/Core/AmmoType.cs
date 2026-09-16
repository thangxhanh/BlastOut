namespace Dev.Scripts.BlastOut.Core
{
    /* Loại đạn quyết định chuyện gì xảy ra khi người chơi chạm màn hình lúc đạn đang bay.
       Thêm loại mới = thêm một nhánh trong BlastProjectile.Detonate, không đụng tới luật chơi. */
    public enum AmmoType
    {
        /* Nổ tại chỗ, đẩy mọi thứ trong bán kính. */
        Bomb,

        /* Tách thành 3 viên bay theo hình quạt, mỗi viên tự nổ khi chạm. */
        Splitter
    }
}
