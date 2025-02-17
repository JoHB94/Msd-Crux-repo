using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using MSD.Crux.Core.IRepositories;
using MSD.Crux.Core.Models;

namespace MSD.Crux.Infra.Repositories;
/// <summary>
///  Part 테이블에 대한 PostgreSQL DB 구현체
/// </summary>
/// <param name="_dbConnection">DIC에서 주입받는 IdbConnection 구현체 객체</param>
public class PartRepoPsqlDb(IDbConnection _dbConnection) : IPartRepo
{

    public async Task<Part?> GetByIdAsync(string partId)
    {
        const string query = "SELECT * FROM part WHERE id = @PartId";
        return await _dbConnection.QuerySingleOrDefaultAsync<Part>(query, new { PartId = partId });
    }


    public async Task AddAsync(Part part)
    {
        const string query = @"
                INSERT INTO part (id, name, type, car_name, model, customer, is_active, note, car_img_path, part_img_path)
                VALUES (@Id, @Name, @Type, @CarName, @Model, @Customer, @IsActive, @Note, @CarImgPath, @PartImgPath)";
        await _dbConnection.ExecuteAsync(query, part);
    }


    public async Task DeleteAsync(string partId)
    {
        const string query = "DELETE FROM part WHERE id = @PartId";
        await _dbConnection.ExecuteAsync(query, new { PartId = partId });
    }
}
