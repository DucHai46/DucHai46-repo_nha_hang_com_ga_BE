using MongoDB.Driver;
using repo_nha_hang_com_ga_BE.Models.MongoDB;
using AutoMapper;
using repo_nha_hang_com_ga_BE.Models.Common.Models.Respond;
using repo_nha_hang_com_ga_BE.Models.Common.Respond;

using MongoDB.Bson;

using Microsoft.Extensions.Options;
using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Paging;
using System.ComponentModel;
using repo_nha_hang_com_ga_BE.Models.Responds.PhuongThucThanhToan;
using repo_nha_hang_com_ga_BE.Models.Requests;

namespace repo_nha_hang_com_ga_BE.Repository.Imp;

public class PhuongThucThanhToanRepository : IPhuongThucThanhToanRepository
{
    private readonly IMongoCollection<PhuongThucThanhToan> _collection;
    private readonly IMapper _mapper;

    public PhuongThucThanhToanRepository(IOptions<MongoDbSettings> settings, IMapper mapper)
    {
        var mongoClientSettings = settings.Value;
        var client = new MongoClient(mongoClientSettings.Connection);
        var database = client.GetDatabase(mongoClientSettings.DatabaseName);
        _collection = database.GetCollection<PhuongThucThanhToan>("PhuongThucThanhToan");
        _mapper = mapper;
    }

    public async Task<RespondAPIPaging<List<PhuongThucThanhToanRespond>>> GetAllPhuongThucThanhToans(RequestSearchPhuongThucThanhToan request)
    {
        try
        {
            var collection = _collection;

            var filter = Builders<PhuongThucThanhToan>.Filter.Empty;
            filter &= Builders<PhuongThucThanhToan>.Filter.Eq(x => x.isDelete, false);

            if (!string.IsNullOrEmpty(request.tenPhuongThuc))
            {
                filter &= Builders<PhuongThucThanhToan>.Filter.Regex(x => x.tenPhuongThuc, new BsonRegularExpression($".*{request.tenPhuongThuc}.*"));

            }


            var projection = Builders<PhuongThucThanhToan>.Projection
                .Include(x => x.tenPhuongThuc)
                .Include(x => x.moTa);

            var findOptions = new FindOptions<PhuongThucThanhToan, PhuongThucThanhToanRespond>
            {
                Projection = projection
            };

            if (request.IsPaging)
            {
                long totalRecord = await collection.CountDocumentsAsync(filter);
                int totalPages = (int)Math.Ceiling((double)totalRecord / request.PageSize);

                int currentPage = request.PageNumber;
                if (currentPage < 1) currentPage = 1;
                if (currentPage > totalPages) currentPage = totalPages;

                findOptions.Skip = (currentPage - 1) * request.PageSize;
                findOptions.Limit = request.PageSize;

                var cursor = await collection.FindAsync(filter, findOptions);
                var PhuongThucThanhToans = await cursor.ToListAsync();

                var pagingDetail = new PagingDetail(currentPage, request.PageSize, totalRecord);
                var pagingResponse = new PagingResponse<List<PhuongThucThanhToanRespond>>
                {
                    Paging = pagingDetail,
                    Data = PhuongThucThanhToans
                };

                return new RespondAPIPaging<List<PhuongThucThanhToanRespond>>(
                    ResultRespond.Succeeded,
                    data: pagingResponse
                );
            }
            else
            {
                var cursor = await collection.FindAsync(filter, findOptions);
                var PhuongThucThanhToans = await cursor.ToListAsync();

                return new RespondAPIPaging<List<PhuongThucThanhToanRespond>>(
                    ResultRespond.Succeeded,
                    data: new PagingResponse<List<PhuongThucThanhToanRespond>>
                    {
                        Data = PhuongThucThanhToans,
                        Paging = new PagingDetail(1, PhuongThucThanhToans.Count, PhuongThucThanhToans.Count)
                    }
                );
            }
        }
        catch (Exception ex)
        {
            return new RespondAPIPaging<List<PhuongThucThanhToanRespond>>(
                ResultRespond.Failed,
                message: ex.Message
            );
        }
    }

    public async Task<RespondAPI<PhuongThucThanhToanRespond>> GetPhuongThucThanhToanById(string id)
    {
        try
        {
            var PhuongThucThanhToan = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();

            if (PhuongThucThanhToan == null)
            {
                return new RespondAPI<PhuongThucThanhToanRespond>(
                    ResultRespond.Failed,
                    message: "Phương thức thanh toán không tồn tại"
                );
            }

            var PhuongThucThanhToanRespond = _mapper.Map<PhuongThucThanhToanRespond>(PhuongThucThanhToan);

            return new RespondAPI<PhuongThucThanhToanRespond>(
                ResultRespond.Succeeded,
                "Lấy thông tin phương thức thanh toán thành công",
                PhuongThucThanhToanRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<PhuongThucThanhToanRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<PhuongThucThanhToanRespond>> CreatePhuongThucThanhToan(RequestAddPhuongThucThanhToan request)
    {
        try
        {
            PhuongThucThanhToan newPhuongThucThanhToan = _mapper.Map<PhuongThucThanhToan>(request);

            newPhuongThucThanhToan.isDelete = false;
            newPhuongThucThanhToan.createdDate = DateTime.Now;
            newPhuongThucThanhToan.updatedDate = DateTime.Now;

            await _collection.InsertOneAsync(newPhuongThucThanhToan);

            var PhuongThucThanhToanRespond = _mapper.Map<PhuongThucThanhToanRespond>(newPhuongThucThanhToan);

            return new RespondAPI<PhuongThucThanhToanRespond>(
                ResultRespond.Succeeded,
                "Thêm phương thức thanh toán thành công",
                PhuongThucThanhToanRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<PhuongThucThanhToanRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<PhuongThucThanhToanRespond>> UpdatePhuongThucThanhToan(string id, RequestUpdatePhuongThucThanhToan request)
    {
        try
        {
            var filter = Builders<PhuongThucThanhToan>.Filter.Eq(x => x.Id, id);
            filter &= Builders<PhuongThucThanhToan>.Filter.Eq(x => x.isDelete, false);
            var PhuongThucThanhToan = await _collection.Find(filter).FirstOrDefaultAsync();

            if (PhuongThucThanhToan == null)
            {
                return new RespondAPI<PhuongThucThanhToanRespond>(
                    ResultRespond.NotFound,
                    "Phương thức thanh toán không tồn tại."
                );
            }

            _mapper.Map(request, PhuongThucThanhToan);

            PhuongThucThanhToan.updatedDate = DateTimeOffset.UtcNow;

            var updateResult = await _collection.ReplaceOneAsync(filter, PhuongThucThanhToan);

            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                return new RespondAPI<PhuongThucThanhToanRespond>(
                    ResultRespond.Error,
                    "Cập nhật phương thức thanh toán không thành công."
                );
            }
            var PhuongThucThanhToanRespond = _mapper.Map<PhuongThucThanhToanRespond>(PhuongThucThanhToan);

            return new RespondAPI<PhuongThucThanhToanRespond>(
                ResultRespond.Succeeded,
                "Cập nhật Phương thức thanh toán thành công",
                PhuongThucThanhToanRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<PhuongThucThanhToanRespond>
            (
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi cập nhật thông tin Phương thức thanh toán: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<string>> DeletePhuongThucThanhToan(string id)
    {
        try
        {
            var existingPhuongThucThanhToan = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();

            if (existingPhuongThucThanhToan == null)
            {
                return new RespondAPI<string>(
                    ResultRespond.NotFound,
                    message: "Phương thức thanh toán không tồn tại"
                );
            }

            var deleteResult = await _collection.DeleteOneAsync(x => x.Id == id);

            if (deleteResult.DeletedCount == 0)
            {
                return new RespondAPI<string>(
                    ResultRespond.Error,
                    "Xóa Phương thức thanh toán không thành công."
                );
            }
            return new RespondAPI<string>(
                ResultRespond.Succeeded,
                "Xóa Phương thức thanh toán thành công.",
                id
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<string>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi xóa Phương thức thanh toán: {ex.Message}"
            );
        }
    }
}