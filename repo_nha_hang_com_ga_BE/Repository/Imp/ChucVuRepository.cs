using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models.Respond;
using repo_nha_hang_com_ga_BE.Models.Common.Paging;
using repo_nha_hang_com_ga_BE.Models.Common.Respond;
using repo_nha_hang_com_ga_BE.Models.MongoDB;
using repo_nha_hang_com_ga_BE.Models.Requests.ChucVu;
using repo_nha_hang_com_ga_BE.Models.Responds.ChucVu;

namespace repo_nha_hang_com_ga_BE.Repository.Imp;

public class ChucVuRepository : IChucVuRepository
{
    private readonly IMongoCollection<ChucVu> _collection;
    private readonly IMapper _mapper;

    public ChucVuRepository(IOptions<MongoDbSettings> settings, IMapper mapper)
    {
        var mongoClientSettings = settings.Value;
        var client = new MongoClient(mongoClientSettings.Connection);
        var database = client.GetDatabase(mongoClientSettings.DatabaseName);
        _collection = database.GetCollection<ChucVu>("ChucVu");
        _mapper = mapper;
    }

    public async Task<RespondAPIPaging<List<ChucVuRespond>>> GetAllChucVus(RequestSearchChucVu request)
    {
        try
        {
            var collection = _collection;

            var filter = Builders<ChucVu>.Filter.Empty;
            filter &= Builders<ChucVu>.Filter.Eq(x => x.isDelete, false);

            if (!string.IsNullOrEmpty(request.tenChucVu))
            {
                filter &= Builders<ChucVu>.Filter.Regex(x => x.tenChucVu, new BsonRegularExpression($".*{request.tenChucVu}.*"));
            }

            var projection = Builders<ChucVu>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenChucVu)
                .Include(x => x.moTa);

            var findOptions = new FindOptions<ChucVu, ChucVuRespond>
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
                var chucVus = await cursor.ToListAsync();

                var pagingDetail = new PagingDetail(currentPage, request.PageSize, totalRecords);
                var pagingResponse = new PagingResponse<List<ChucVuRespond>>
                {
                    Paging = pagingDetail,
                    Data = chucVus
                };

                return new RespondAPIPaging<List<ChucVuRespond>>(
                    ResultRespond.Succeeded,
                    data: pagingResponse
                );
            }
            else
            {
                var cursor = await collection.FindAsync(filter, findOptions);
                var chucVus = await cursor.ToListAsync();

                return new RespondAPIPaging<List<ChucVuRespond>>(
                    ResultRespond.Succeeded,
                    data: new PagingResponse<List<ChucVuRespond>>
                    {
                        Data = chucVus,
                        Paging = new PagingDetail(1, chucVus.Count, chucVus.Count)
                    }
                );
            }
        }
        catch (Exception ex)
        {
            return new RespondAPIPaging<List<ChucVuRespond>>(
                ResultRespond.Error,
                message: ex.Message
            );
        }
    }

    public async Task<RespondAPI<ChucVuRespond>> GetChucVuById(string id)
    {
        try
        {
            var chucVu = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();

            if (chucVu == null)
            {
                return new RespondAPI<ChucVuRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy chức vụ với ID đã cung cấp."
                );
            }

            var chucVuRespond = _mapper.Map<ChucVuRespond>(chucVu);

            return new RespondAPI<ChucVuRespond>(
                ResultRespond.Succeeded,
                "Lấy chức vụ thành công.",
                chucVuRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<ChucVuRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<ChucVuRespond>> CreateChucVu(RequestAddChucVu request)
    {
        try
        {
            ChucVu newChucVu = _mapper.Map<ChucVu>(request);

            newChucVu.createdDate = DateTimeOffset.UtcNow;
            newChucVu.updatedDate = DateTimeOffset.UtcNow;
            newChucVu.isDelete = false;
            // Thiết lập createdUser và updatedUser nếu có thông tin người dùng
            // newDanhMucMonAn.createdUser = currentUser.Id;
            // newDanhMucNguyenLieu.updatedUser = currentUser.Id;

            await _collection.InsertOneAsync(newChucVu);

            var chucVuRespond = _mapper.Map<ChucVuRespond>(newChucVu);

            return new RespondAPI<ChucVuRespond>(
                ResultRespond.Succeeded,
                "Tạo chức vụ thành công.",
                chucVuRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<ChucVuRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi tạo chức vụ: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<ChucVuRespond>> UpdateChucVu(string id, RequestUpdateChucVu request)
    {
        try
        {
            var filter = Builders<ChucVu>.Filter.Eq(x => x.Id, id);
            filter &= Builders<ChucVu>.Filter.Eq(x => x.isDelete, false);
            var chucVu = await _collection.Find(filter).FirstOrDefaultAsync();

            if (chucVu == null)
            {
                return new RespondAPI<ChucVuRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy chức vụ với ID đã cung cấp."
                );
            }

            _mapper.Map(request, chucVu);

            chucVu.updatedDate = DateTimeOffset.UtcNow;

            // Cập nhật người dùng nếu có thông tin
            // danhMucNguyenLieu.updatedUser = currentUser.Id;

            var updateResult = await _collection.ReplaceOneAsync(filter, chucVu);

            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                return new RespondAPI<ChucVuRespond>(
                    ResultRespond.Error,
                    "Cập nhật chức vụ không thành công."
                );
            }

            var chucVuRespond = _mapper.Map<ChucVuRespond>(chucVu);

            return new RespondAPI<ChucVuRespond>(
                ResultRespond.Succeeded,
                "Cập nhật chức vụ thành công.",
                chucVuRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<ChucVuRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi cập nhật chức vụ: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<string>> DeleteChucVu(string id)
    {
        try
        {
            var existingChucVu = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();
            if (existingChucVu == null)
            {
                return new RespondAPI<string>(
                    ResultRespond.NotFound,
                    "Không tìm thấy chức vụ để xóa."
                );
            }

            var deleteResult = await _collection.DeleteOneAsync(x => x.Id == id);

            if (deleteResult.DeletedCount == 0)
            {
                return new RespondAPI<string>(
                    ResultRespond.Error,
                    "Xóa chức vụ không thành công."
                );
            }

            return new RespondAPI<string>(
                ResultRespond.Succeeded,
                "Xóa chức vụ thành công.",
                id
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<string>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi xóa chức vụ: {ex.Message}"
            );
        }
    }
}