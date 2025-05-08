using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models.Respond;
using repo_nha_hang_com_ga_BE.Models.Common.Paging;
using repo_nha_hang_com_ga_BE.Models.Common.Respond;
using repo_nha_hang_com_ga_BE.Models.MongoDB;
using repo_nha_hang_com_ga_BE.Models.Requests.LoaiDon;
using repo_nha_hang_com_ga_BE.Models.Requests.ThucDon;
using repo_nha_hang_com_ga_BE.Models.Responds.LoaiDon;

namespace repo_nha_hang_com_ga_BE.Models.Repositories.Imp;

public class LoaiDonRepository : ILoaiDonRepository
{
    private readonly IMongoCollection<LoaiDon> _collection;
    private readonly IMapper _mapper;

    public LoaiDonRepository(IOptions<MongoDbSettings> settings, IMapper mapper)
    {
        var mongoClientSettings = settings.Value;
        var client = new MongoClient(mongoClientSettings.Connection);
        var database = client.GetDatabase(mongoClientSettings.DatabaseName);
        _collection = database.GetCollection<LoaiDon>("LoaiDon");
        _mapper = mapper;
    }

    public async Task<RespondAPIPaging<List<LoaiDonRespond>>> GetAllLoaiDon(RequestSearchLoaiDon request)
    {
        try
        {
            var collection = _collection;

            var filter = Builders<LoaiDon>.Filter.Empty;
            filter &= Builders<LoaiDon>.Filter.Eq(x => x.isDelete, false);

            if (!string.IsNullOrEmpty(request.tenLoaiDon))
            {
                filter &= Builders<LoaiDon>.Filter.Regex(x => x.tenLoaiDon, new BsonRegularExpression($".*{request.tenLoaiDon}.*"));
            }

            var projection = Builders<LoaiDon>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenLoaiDon)
                .Include(x => x.moTa);


            var findOptions = new FindOptions<LoaiDon, LoaiDonRespond>
            {
                Projection = projection
            };

            if (request.IsPaging)
            {
                long totalRecords = await collection.CountDocumentsAsync(filter);

                int totalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize);

                int currentPage = request.PageNumber;
                if (currentPage < 1) currentPage = 1;
                if (currentPage > totalPages) currentPage = totalPages;

                findOptions.Skip = (currentPage - 1) * request.PageSize;
                findOptions.Limit = request.PageSize;

                var cursor = await collection.FindAsync(filter, findOptions);
                var LoaiDons = await cursor.ToListAsync();

                var pagingDetail = new PagingDetail(currentPage, request.PageSize, totalRecords);
                var pagingResponse = new PagingResponse<List<LoaiDonRespond>>
                {
                    Paging = pagingDetail,
                    Data = LoaiDons
                };

                return new RespondAPIPaging<List<LoaiDonRespond>>(
                    ResultRespond.Succeeded,
                    data: pagingResponse
                );
            }
            else
            {
                var cursor = await collection.FindAsync(filter, findOptions);
                var LoaiDons = await cursor.ToListAsync();

                return new RespondAPIPaging<List<LoaiDonRespond>>(
                    ResultRespond.Succeeded,
                    data: new PagingResponse<List<LoaiDonRespond>>
                    {
                        Data = LoaiDons,
                        Paging = new PagingDetail(1, LoaiDons.Count, LoaiDons.Count)
                    }
                );
            }
        }
        catch (Exception ex)
        {
            return new RespondAPIPaging<List<LoaiDonRespond>>(
                ResultRespond.Error,
                message: ex.Message
            );
        }
    }

    public async Task<RespondAPI<LoaiDonRespond>> GetLoaiDonById(string id)
    {
        try
        {
            var LoaiDon = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();

            if (LoaiDon == null)
            {
                return new RespondAPI<LoaiDonRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy loại đơn với ID đã cung cấp."
                );
            }

            var LoaiDonRespond = _mapper.Map<LoaiDonRespond>(LoaiDon);

            return new RespondAPI<LoaiDonRespond>(
                ResultRespond.Succeeded,
                "Lấy loại đơn thành công.",
                LoaiDonRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<LoaiDonRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<LoaiDonRespond>> CreateLoaiDon(RequestAddLoaiDon request)
    {
        try
        {
            LoaiDon newLoaiDon = _mapper.Map<LoaiDon>(request);

            newLoaiDon.createdDate = DateTimeOffset.UtcNow;
            newLoaiDon.updatedDate = DateTimeOffset.UtcNow;
            newLoaiDon.isDelete = false;
            // Thiết lập createdUser và updatedUser nếu có thông tin người dùng
            // newDanhMucMonAn.createdUser = currentUser.Id;
            // newDanhMucNguyenLieu.updatedUser = currentUser.Id;

            await _collection.InsertOneAsync(newLoaiDon);

            var LoaiDonRespond = _mapper.Map<LoaiDonRespond>(newLoaiDon);

            return new RespondAPI<LoaiDonRespond>(
                ResultRespond.Succeeded,
                "Tạo loại đơn thành công.",
                LoaiDonRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<LoaiDonRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi tạo loại đơn: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<LoaiDonRespond>> UpdateLoaiDon(string id, RequestUpdateLoaiDon request)
    {
        try
        {
            var filter = Builders<LoaiDon>.Filter.Eq(x => x.Id, id);
            filter &= Builders<LoaiDon>.Filter.Eq(x => x.isDelete, false);
            var LoaiDon = await _collection.Find(filter).FirstOrDefaultAsync();

            if (LoaiDon == null)
            {
                return new RespondAPI<LoaiDonRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy loại đơn với ID đã cung cấp."
                );
            }

            _mapper.Map(request, LoaiDon);

            LoaiDon.updatedDate = DateTimeOffset.UtcNow;

            // Cập nhật người dùng nếu có thông tin
            // danhMucNguyenLieu.updatedUser = currentUser.Id;

            var updateResult = await _collection.ReplaceOneAsync(filter, LoaiDon);

            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                return new RespondAPI<LoaiDonRespond>(
                    ResultRespond.Error,
                    "Cập nhật loại đơn không thành công."
                );
            }

            var LoaiDonRespond = _mapper.Map<LoaiDonRespond>(LoaiDon);

            return new RespondAPI<LoaiDonRespond>(
                ResultRespond.Succeeded,
                "Cập nhật loại đơn thành công.",
                LoaiDonRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<LoaiDonRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi cập nhật loại đơn: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<string>> DeleteLoaiDon(string id)
    {
        try
        {
            var existingLoaiDon = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();
            if (existingLoaiDon == null)
            {
                return new RespondAPI<string>(
                    ResultRespond.NotFound,
                    "Không tìm thấy loại đơn để xóa."
                );
            }

            var deleteResult = await _collection.DeleteOneAsync(x => x.Id == id);

            if (deleteResult.DeletedCount == 0)
            {
                return new RespondAPI<string>(
                    ResultRespond.Error,
                    "Xóa loại đơn không thành công."
                );
            }

            return new RespondAPI<string>(
                ResultRespond.Succeeded,
                "Xóa loại đơn thành công.",
                id
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<string>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi xóa loại đơn: {ex.Message}"
            );
        }
    }


}