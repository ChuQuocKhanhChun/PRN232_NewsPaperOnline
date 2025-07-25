using GrpcArticleService;
using System.ComponentModel.DataAnnotations;

namespace PRN232_FinalProject_Client.Models
{
    public class StatisticViewModel
    {
        [DataType(DataType.Date)]
        [Display(Name = "Từ ngày")]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Đến ngày")]
        public DateTime? EndDate { get; set; }

        public List<MostViewedArticleModel> MostViewedArticles { get; set; } = new List<MostViewedArticleModel>();
        public int TotalArticleViews { get; set; }
        public int NewUsersCount { get; set; }
        public List<MostLikedArticleModel> MostLikedArticles { get; set; } = new List<MostLikedArticleModel>();
        // public int TotalComments { get; set; } // Nếu có thống kê bình luận
        // public int TotalLikes { get; set; } // Nếu có thống kê likes
    }
}
