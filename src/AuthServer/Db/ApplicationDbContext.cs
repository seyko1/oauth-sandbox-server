using Microsoft.EntityFrameworkCore;

namespace AuthServer.Db;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options);