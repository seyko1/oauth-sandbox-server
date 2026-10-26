using Microsoft.EntityFrameworkCore;

namespace AuthServer.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options);