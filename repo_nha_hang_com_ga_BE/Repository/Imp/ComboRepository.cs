using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models.Respond;
using repo_nha_hang_com_ga_BE.Models.Common.Paging;
using repo_nha_hang_com_ga_BE.Models.Common.Respond;
using repo_nha_hang_com_ga_BE.Models.MongoDB;
using repo_nha_hang_com_ga_BE.Models.Requests.Combo;
using repo_nha_hang_com_ga_BE.Models.Responds.Combo;

namespace repo_nha_hang_com_ga_BE.Repository.Imp;

public class ComboRepository : IComboRepository
{
    private readonly IMongoCollection<Combo> _collection;
    private readonly IMapper _mapper;

    public ComboRepository(IOptions<MongoDbSettings> settings, IMapper mapper)
    {
        var mongoClientSettings = settings.Value;
        var client = new MongoClient(mongoClientSettings.Connection);
        var database = client.GetDatabase(mongoClientSettings.DatabaseName);
        _collection = database.GetCollection<Combo>("Combo");
        _mapper = mapper;
    }

    public async Task<RespondAPIPaging<List<ComboRespond>>> GetAllCombos(RequestSearchCombo request)
    {
        try
        {
            var collection = _collection;

            var filter = Builders<Combo>.Filter.Empty;
            filter &= Builders<Combo>.Filter.Eq(x => x.isDelete, false);

            if (!string.IsNullOrEmpty(request.tenCombo))
            {
                filter &= Builders<Combo>.Filter.Regex(x => x.tenCombo, new BsonRegularExpression($".*{request.tenCombo}.*"));
            }

            if (request.giaTien.HasValue)
            {
                filter &= Builders<Combo>.Filter.Regex(x => x.giaTien, new BsonRegularExpression($".*{request.giaTien}.*"));
            }

            var projection = Builders<Combo>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenCombo)
                .Include(x => x.loaiMonAns)
                .Include(x => x.hinhAnh)
                .Include(x => x.giaTien)
                .Include(x => x.moTa);

            var findOptions = new FindOptions<Combo, ComboRespond>
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
                var combos = await cursor.ToListAsync();

                var pagingDetail = new PagingDetail(currentPage, request.PageSize, totalRecords);
                var pagingResponse = new PagingResponse<List<ComboRespond>>
                {
                    Paging = pagingDetail,
                    Data = combos
                };

                return new RespondAPIPaging<List<ComboRespond>>(
                    ResultRespond.Succeeded,
                    data: pagingResponse
                );
            }
            else
            {
                var cursor = await collection.FindAsync(filter, findOptions);
                var combos = await cursor.ToListAsync();

                return new RespondAPIPaging<List<ComboRespond>>(
                    ResultRespond.Succeeded,
                    data: new PagingResponse<List<ComboRespond>>
                    {
                        Data = combos,
                        Paging = new PagingDetail(1, combos.Count, combos.Count)
                    }
                );
            }
        }
        catch (Exception ex)
        {
            return new RespondAPIPaging<List<ComboRespond>>(
                ResultRespond.Error,
                message: ex.Message
            );
        }
    }

    public async Task<RespondAPI<ComboRespond>> GetComboById(string id)
    {
        try
        {
            var combo = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();

            if (combo == null)
            {
                return new RespondAPI<ComboRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy combo với ID đã cung cấp."
                );
            }

            var comboRespond = _mapper.Map<ComboRespond>(combo);

            return new RespondAPI<ComboRespond>(
                ResultRespond.Succeeded,
                "Lấy combo thành công.",
                comboRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<ComboRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<ComboRespond>> CreateCombo(RequestAddCombo request)
    {
        try
        {
            Combo newCombo = _mapper.Map<Combo>(request);

            newCombo.createdDate = DateTimeOffset.UtcNow;
            newCombo.updatedDate = DateTimeOffset.UtcNow;
            newCombo.isDelete = false;
            // Thiết lập createdUser và updatedUser nếu có thông tin người dùng
            // newDanhMucMonAn.createdUser = currentUser.Id;
            // newDanhMucNguyenLieu.updatedUser = currentUser.Id;

            await _collection.InsertOneAsync(newCombo);

            var comboRespond = _mapper.Map<ComboRespond>(newCombo);

            return new RespondAPI<ComboRespond>(
                ResultRespond.Succeeded,
                "Tạo combo thành công.",
                comboRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<ComboRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi tạo combo: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<ComboRespond>> UpdateCombo(string id, RequestUpdateCombo request)
    {
        try
        {
            var filter = Builders<Combo>.Filter.Eq(x => x.Id, id);
            filter &= Builders<Combo>.Filter.Eq(x => x.isDelete, false);
            var combo = await _collection.Find(filter).FirstOrDefaultAsync();

            if (combo == null)
            {
                return new RespondAPI<ComboRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy combo với ID đã cung cấp."
                );
            }

            _mapper.Map(request, combo);

            combo.updatedDate = DateTimeOffset.UtcNow;

            // Cập nhật người dùng nếu có thông tin
            // danhMucNguyenLieu.updatedUser = currentUser.Id;

            var updateResult = await _collection.ReplaceOneAsync(filter, combo);

            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                return new RespondAPI<ComboRespond>(
                    ResultRespond.Error,
                    "Cập nhật combo không thành công."
                );
            }

            var comboRespond = _mapper.Map<ComboRespond>(combo);

            return new RespondAPI<ComboRespond>(
                ResultRespond.Succeeded,
                "Cập nhật combo thành công.",
                comboRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<ComboRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi cập nhật combo: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<string>> DeleteCombo(string id)
    {
        try
        {
            var existingDanhMuc = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();
            if (existingDanhMuc == null)
            {
                return new RespondAPI<string>(
                    ResultRespond.NotFound,
                    "Không tìm thấy combo để xóa."
                );
            }

            var deleteResult = await _collection.DeleteOneAsync(x => x.Id == id);

            if (deleteResult.DeletedCount == 0)
            {
                return new RespondAPI<string>(
                    ResultRespond.Error,
                    "Xóa combo không thành công."
                );
            }

            return new RespondAPI<string>(
                ResultRespond.Succeeded,
                "Xóa combo thành công.",
                id
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<string>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi xóa combo: {ex.Message}"
            );
        }
    }
}