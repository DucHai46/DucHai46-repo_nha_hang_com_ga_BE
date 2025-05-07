using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models;
using repo_nha_hang_com_ga_BE.Models.Common.Models.Respond;
using repo_nha_hang_com_ga_BE.Models.Common.Paging;
using repo_nha_hang_com_ga_BE.Models.Common.Respond;
using repo_nha_hang_com_ga_BE.Models.MongoDB;
using repo_nha_hang_com_ga_BE.Models.Requests.Ban;
using repo_nha_hang_com_ga_BE.Models.Responds.Ban;

namespace repo_nha_hang_com_ga_BE.Repository.Imp;

public class BanRepository : IBanRepository
{
    private readonly IMongoCollection<Ban> _collection;
    private readonly IMongoCollection<LoaiBan> _collectionLoaiBan;
    private readonly IMapper _mapper;

    public BanRepository(IOptions<MongoDbSettings> settings, IMapper mapper)
    {
        var mongoClientSettings = settings.Value;
        var client = new MongoClient(mongoClientSettings.Connection);
        var database = client.GetDatabase(mongoClientSettings.DatabaseName);
        _collection = database.GetCollection<Ban>("Ban");
        _collectionLoaiBan = database.GetCollection<LoaiBan>("LoaiBan");
        _mapper = mapper;
    }

    public async Task<RespondAPIPaging<List<BanRespond>>> GetAllBans(RequestSearchBan request)
    {
        try
        {
            var collection = _collection;

            var filter = Builders<Ban>.Filter.Empty;
            filter &= Builders<Ban>.Filter.Eq(x => x.isDelete, false);

            if (!string.IsNullOrEmpty(request.tenBan))
            {
                filter &= Builders<Ban>.Filter.Regex(x => x.tenBan, new BsonRegularExpression($".*{request.tenBan}.*"));
            }

            if (!string.IsNullOrEmpty(request.idLoaiBan))
            {
                filter &= Builders<Ban>.Filter.Eq(x => x.loaiBan, request.idLoaiBan);
            }

            if (request.trangThai != null)
            {
                filter &= Builders<Ban>.Filter.Eq(x => x.trangThai, request.trangThai);
            }

            var projection = Builders<Ban>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenBan)
                .Include(x => x.loaiBan)
                .Include(x => x.trangThai);

            var findOptions = new FindOptions<Ban, Ban>
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
                var bans = await cursor.ToListAsync();

                // Lấy danh sách ID loại bàn
                var loaiBanIds = bans.Select(x => x.loaiBan).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                // Query bảng loại bàn
                var loaiBanFilter = Builders<LoaiBan>.Filter.In(x => x.Id, loaiBanIds);
                var loaiBanProjection = Builders<LoaiBan>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenLoai);
                var loaiBans = await _collectionLoaiBan.Find(loaiBanFilter)
                    .Project<LoaiBan>(loaiBanProjection)
                    .ToListAsync();

                // Tạo dictionary để map nhanh
                var loaiBanDict = loaiBans.ToDictionary(x => x.Id, x => x.tenLoai);

                // Map dữ liệu
                var banResponds = bans.Select(ban => new BanRespond
                {
                    id = ban.Id,
                    tenBan = ban.tenBan,
                    trangThai = ban.trangThai,
                    loaiBan = new IdName
                    {
                        Id = ban.loaiBan,
                        Name = ban.loaiBan != null && loaiBanDict.ContainsKey(ban.loaiBan) ? loaiBanDict[ban.loaiBan] : null
                    }
                }).ToList();

                var pagingDetail = new PagingDetail(currentPage, request.PageSize, totalRecords);
                var pagingResponse = new PagingResponse<List<BanRespond>>
                {
                    Paging = pagingDetail,
                    Data = banResponds
                };

                return new RespondAPIPaging<List<BanRespond>>(
                    ResultRespond.Succeeded,
                    data: pagingResponse
                );
            }
            else
            {
                var cursor = await collection.FindAsync(filter, findOptions);
                var bans = await cursor.ToListAsync();

                // Lấy danh sách ID loại bàn
                var loaiBanIds = bans.Select(x => x.loaiBan).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                // Query bảng loại bàn
                var loaiBanFilter = Builders<LoaiBan>.Filter.In(x => x.Id, loaiBanIds);
                var loaiBanProjection = Builders<LoaiBan>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenLoai);
                var loaiBans = await _collectionLoaiBan.Find(loaiBanFilter)
                    .Project<LoaiBan>(loaiBanProjection)
                    .ToListAsync();

                // Tạo dictionary để map nhanh
                var loaiBanDict = loaiBans.ToDictionary(x => x.Id, x => x.tenLoai);

                // Map dữ liệu
                var banResponds = bans.Select(ban => new BanRespond
                {
                    id = ban.Id,
                    tenBan = ban.tenBan,
                    trangThai = ban.trangThai,
                    loaiBan = new IdName
                    {
                        Id = ban.loaiBan,
                        Name = ban.loaiBan != null && loaiBanDict.ContainsKey(ban.loaiBan) ? loaiBanDict[ban.loaiBan] : null
                    }
                }).ToList();

                return new RespondAPIPaging<List<BanRespond>>(
                    ResultRespond.Succeeded,
                    data: new PagingResponse<List<BanRespond>>
                    {
                        Data = banResponds,
                        Paging = new PagingDetail(1, banResponds.Count(), banResponds.Count())
                    }
                );
            }
        }
        catch (Exception ex)
        {
            return new RespondAPIPaging<List<BanRespond>>(
                ResultRespond.Error,
                message: ex.Message
            );
        }
    }

    public async Task<RespondAPI<BanRespond>> GetBanById(string id)
    {
        try
        {
            var ban = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();

            if (ban == null)
            {
                return new RespondAPI<BanRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy bàn với ID đã cung cấp."
                );
            }
            var loaiBan = await _collectionLoaiBan.Find(x => x.Id == ban.loaiBan).FirstOrDefaultAsync();
            var banRespond = _mapper.Map<BanRespond>(ban);
            banRespond.loaiBan = new IdName
            {
                Id = loaiBan.Id,
                Name = loaiBan.tenLoai
            };

            return new RespondAPI<BanRespond>(
                ResultRespond.Succeeded,
                "Lấy bàn thành công.",
                banRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<BanRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<BanRespond>> CreateBan(RequestAddBan request)
    {
        try
        {
            Ban newBan = _mapper.Map<Ban>(request);

            newBan.createdDate = DateTimeOffset.UtcNow;
            newBan.updatedDate = DateTimeOffset.UtcNow;
            newBan.isDelete = false;
            // Thiết lập createdUser và updatedUser nếu có thông tin người dùng
            // newDanhMucMonAn.createdUser = currentUser.Id;
            // newDanhMucNguyenLieu.updatedUser = currentUser.Id;

            await _collection.InsertOneAsync(newBan);
            var loaiBan = await _collectionLoaiBan.Find(x => x.Id == newBan.loaiBan).FirstOrDefaultAsync();
            var banRespond = _mapper.Map<BanRespond>(newBan);
            banRespond.loaiBan = new IdName
            {
                Id = loaiBan.Id,
                Name = loaiBan.tenLoai
            };

            return new RespondAPI<BanRespond>(
                ResultRespond.Succeeded,
                "Tạo bàn thành công.",
                banRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<BanRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi tạo bàn: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<BanRespond>> UpdateBan(string id, RequestUpdateBan request)
    {
        try
        {
            var filter = Builders<Ban>.Filter.Eq(x => x.Id, id);
            filter &= Builders<Ban>.Filter.Eq(x => x.isDelete, false);
            var ban = await _collection.Find(filter).FirstOrDefaultAsync();

            if (ban == null)
            {
                return new RespondAPI<BanRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy bàn với ID đã cung cấp."
                );
            }

            _mapper.Map(request, ban);

            ban.updatedDate = DateTimeOffset.UtcNow;

            // Cập nhật người dùng nếu có thông tin
            // danhMucNguyenLieu.updatedUser = currentUser.Id;

            var updateResult = await _collection.ReplaceOneAsync(filter, ban);

            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                return new RespondAPI<BanRespond>(
                    ResultRespond.Error,
                    "Cập nhật bàn không thành công."
                );
            }

            var banRespond = _mapper.Map<BanRespond>(ban);

            return new RespondAPI<BanRespond>(
                ResultRespond.Succeeded,
                "Cập nhật bàn thành công.",
                banRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<BanRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi cập nhật bàn: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<string>> DeleteBan(string id)
    {
        try
        {
            var existingBan = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();
            if (existingBan == null)
            {
                return new RespondAPI<string>(
                    ResultRespond.NotFound,
                    "Không tìm thấy bàn để xóa."
                );
            }

            var deleteResult = await _collection.DeleteOneAsync(x => x.Id == id);

            if (deleteResult.DeletedCount == 0)
            {
                return new RespondAPI<string>(
                    ResultRespond.Error,
                    "Xóa bàn không thành công."
                );
            }

            return new RespondAPI<string>(
                ResultRespond.Succeeded,
                "Xóa bàn thành công.",
                id
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<string>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi xóa bàn: {ex.Message}"
            );
        }
    }
}