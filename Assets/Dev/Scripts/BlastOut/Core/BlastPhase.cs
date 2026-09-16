namespace Dev.Scripts.BlastOut.Core
{
    /* Trạng thái một lượt chơi. Thứ tự khai báo theo đúng thứ tự xảy ra trong một phát bắn. */
    public enum BlastPhase
    {
        /* Chờ người chơi kéo để ngắm. Đây là trạng thái duy nhất nhận input ngắm. */
        Aiming,

        /* Đạn đang bay. Chạm màn hình lúc này = kích nổ — trục quyết định chính của game. */
        Flying,

        /* Đã nổ, đang chờ vật lý lắng xuống trước khi kết luận thắng/thua. */
        Resolving,

        Won,
        Lost
    }
}
