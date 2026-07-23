using Template.Core.Models.Dashboard;

namespace Template.Core.Repository.Dashboard;

public interface IDashboardRepository
{
    Task<StaffDashboardViewModel> GetStaffDashboardAsync(Guid userId);
    Task<InnovationTeamDashboardViewModel> GetInnovationTeamDashboardAsync();
    Task<ItAdminDashboardViewModel> GetItAdminDashboardAsync();
}
