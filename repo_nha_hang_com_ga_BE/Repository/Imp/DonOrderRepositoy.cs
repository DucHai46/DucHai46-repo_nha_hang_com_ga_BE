
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
using repo_nha_hang_com_ga_BE.Models.Requests.DonOrder;
using repo_nha_hang_com_ga_BE.Models.Responds.DonOrder;


namespace repo_nha_hang_com_ga_BE.Repository.Imp;

public class DonOrderRepository : IDonOrderRepository
{
    private readonly IMongoCollection<DonOrder> _collection;
    private readonly IMongoCollection<LoaiBan> _collectionLoaiBan;
    private readonly IMongoCollection<LoaiDon> _collectionLoaiDon;

    private readonly IMapper _mapper;

    public DonOrderRepository(IOptions<MongoDbSettings> settings, IMapper mapper)
    {
        var mongoClientSettings = settings.Value;
        var client = new MongoClient(mongoClientSettings.Connection);
        var database = client.GetDatabase(mongoClientSettings.DatabaseName);
        _collection = database.GetCollection<DonOrder>("DonOrder");
        _collectionLoaiBan = database.GetCollection<LoaiBan>("LoaiBan");
        _collectionLoaiDon = database.GetCollection<LoaiDon>("LoaiDon");
        _mapper = mapper;
    }

    public async Task<RespondAPIPaging<List<DonOrderRespond>>> GetAllDonOrder(RequestSearchDonOrder request)
    {
        try
        {
            var collection = _collection;

            var filter = Builders<DonOrder>.Filter.Empty;
            filter &= Builders<DonOrder>.Filter.Eq(x => x.isDelete, false);

            if (!string.IsNullOrEmpty(request.tenDon))
            {
                filter &= Builders<DonOrder>.Filter.Regex(x => x.tenDon, request.tenDon);
            }

            if (!string.IsNullOrEmpty(request.loaiDon))
            {
                filter &= Builders<DonOrder>.Filter.Regex(x => x.loaiDon, request.loaiDon);
                // filter &= Builders<DonOrder>.Filter.Regex(x => x.khachHang!.Name, new BsonRegularExpression($".*{request.khachHangName}.*"));
            }

            if (!string.IsNullOrEmpty(request.ban))
            {
                filter &= Builders<DonOrder>.Filter.Eq(x => x.ban, request.ban);
            }

            if (request.trangThai.HasValue) // Kiểm tra nếu trangThai có giá trị: True hoặc False
            {
                filter &= Builders<DonOrder>.Filter.Eq(x => x.trangThai, request.trangThai);
            }

            var projection = Builders<DonOrder>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenDon)
                .Include(x => x.loaiDon)
                .Include(x => x.ban)
                .Include(x => x.trangThai)
                .Include(x => x.chiTietDonOrder)
                .Include(x => x.tongTien);

            var findOptions = new FindOptions<DonOrder, DonOrder>
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
                // var bans = await cursor.ToListAsync();
                var dons = await cursor.ToListAsync();

                // Lấy danh sách ID loại bàn
                var loaiBanIds = dons.Select(x => x.ban).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                // Lấy danh sách ID loại đơn order 
                var loaiDonIds = dons.Select(x => x.loaiDon).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                // Query bảng loại bàn
                var loaiBanFilter = Builders<LoaiBan>.Filter.In(x => x.Id, loaiBanIds);
                var loaiBanProjection = Builders<LoaiBan>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenLoai);
                var loaiBans = await _collectionLoaiBan.Find(loaiBanFilter)
                    .Project<LoaiBan>(loaiBanProjection)
                    .ToListAsync();

                // Query bảng loại đơn
                var loaiDonFilter = Builders<LoaiDon>.Filter.In(x => x.Id, loaiDonIds);
                var loaiDonProjection = Builders<LoaiDon>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenLoaiDon);
                var loaiDons = await _collectionLoaiDon.Find(loaiDonFilter)
                    .Project<LoaiDon>(loaiDonProjection)
                    .ToListAsync();

                // Tạo dictionary để map nhanh
                var loaiBanDict = loaiBans.ToDictionary(x => x.Id, x => x.tenLoai);
                var loaiDonDict = loaiDons.ToDictionary(x => x.Id, x => x.tenLoaiDon);

                // Map dữ liệu
                var donOrderResponds = dons.Select(donOrder => new DonOrderRespond
                {
                    id = donOrder.Id,
                    tenDon = donOrder.tenDon,
                    loaiDon = new IdName
                    {
                        Id = donOrder.loaiDon,
                        Name = donOrder.loaiDon != null && loaiDonDict.ContainsKey(donOrder.loaiDon) ? loaiDonDict[donOrder.loaiDon] : null
                    },
                    ban = new IdName
                    {
                        Id = donOrder.ban,
                        Name = donOrder.ban != null && loaiBanDict.ContainsKey(donOrder.ban) ? loaiBanDict[donOrder.ban] : null
                    },
                    trangThai = donOrder.trangThai,
                }).ToList();

                var pagingDetail = new PagingDetail(currentPage, request.PageSize, totalRecords);
                var pagingResponse = new PagingResponse<List<DonOrderRespond>>
                {
                    Paging = pagingDetail,
                    Data = donOrderResponds
                };

                return new RespondAPIPaging<List<DonOrderRespond>>(
                    ResultRespond.Succeeded,
                    data: pagingResponse
                );
            }
            else
            {
                var cursor = await collection.FindAsync(filter, findOptions);
                // var bans = await cursor.ToListAsync();
                var dons = await cursor.ToListAsync();

                // Lấy danh sách ID loại bàn
                var loaiBanIds = dons.Select(x => x.ban).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                // Lấy danh sách ID loại đơn order 
                var loaiDonIds = dons.Select(x => x.loaiDon).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                // Query bảng loại bàn
                var loaiBanFilter = Builders<LoaiBan>.Filter.In(x => x.Id, loaiBanIds);
                var loaiBanProjection = Builders<LoaiBan>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenLoai);
                var loaiBans = await _collectionLoaiBan.Find(loaiBanFilter)
                    .Project<LoaiBan>(loaiBanProjection)
                    .ToListAsync();

                // Query bảng loại đơn
                var loaiDonFilter = Builders<LoaiDon>.Filter.In(x => x.Id, loaiDonIds);
                var loaiDonProjection = Builders<LoaiDon>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenLoaiDon);
                var loaiDons = await _collectionLoaiDon.Find(loaiDonFilter)
                    .Project<LoaiDon>(loaiDonProjection)
                    .ToListAsync();

                // Tạo dictionary để map nhanh
                var loaiBanDict = loaiBans.ToDictionary(x => x.Id, x => x.tenLoai);
                var loaiDonDict = loaiDons.ToDictionary(x => x.Id, x => x.tenLoaiDon);

                // Map dữ liệu
                var donOrderResponds = dons.Select(donOrder => new DonOrderRespond
                {
                    id = donOrder.Id,
                    tenDon = donOrder.tenDon,
                    loaiDon = new IdName
                    {
                        Id = donOrder.loaiDon,
                        Name = donOrder.loaiDon != null && loaiDonDict.ContainsKey(donOrder.loaiDon) ? loaiDonDict[donOrder.loaiDon] : null
                    },
                    ban = new IdName
                    {
                        Id = donOrder.ban,
                        Name = donOrder.ban != null && loaiBanDict.ContainsKey(donOrder.ban) ? loaiBanDict[donOrder.ban] : null
                    },
                    trangThai = donOrder.trangThai,
                }).ToList();

                return new RespondAPIPaging<List<DonOrderRespond>>(
                    ResultRespond.Succeeded,
                    data: new PagingResponse<List<DonOrderRespond>>
                    {
                        Data = donOrderResponds,
                        Paging = new PagingDetail(1, donOrderResponds.Count(), donOrderResponds.Count())
                    }
                );
            }
        }
        catch (Exception ex)
        {
            return new RespondAPIPaging<List<DonOrderRespond>>(
                ResultRespond.Error,
                message: ex.Message
            );
        }
    }

    public async Task<RespondAPI<DonOrderRespond>> GetDonOrderById(string id)
    {
        try
        {
            var donOrder = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();

            if (donOrder == null)
            {
                return new RespondAPI<DonOrderRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy đơn order với ID đã cung cấp."
                );
            }

            var loaiDon = await _collectionLoaiDon.Find(x => x.Id == donOrder.loaiDon).FirstOrDefaultAsync();
            var donOrderRespond = _mapper.Map<DonOrderRespond>(donOrder);

            donOrderRespond.loaiDon = new IdName
            {
                Id = loaiDon.Id,
                Name = loaiDon.tenLoaiDon
            };

            return new RespondAPI<DonOrderRespond>(
                ResultRespond.Succeeded,
                "Lấy đơn order thành công.",
                donOrderRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<DonOrderRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<DonOrderRespond>> CreateDonOrder(RequestAddDonOrder request)
    {
        try
        {
            DonOrder newDonOrder = _mapper.Map<DonOrder>(request);

            newDonOrder.createdDate = DateTimeOffset.UtcNow;
            newDonOrder.updatedDate = DateTimeOffset.UtcNow;
            newDonOrder.isDelete = false;
            // Thiết lập createdUser và updatedUser nếu có thông tin người dùng
            // newDanhMucMonAn.createdUser = currentUser.Id;
            // newDanhMucNguyenLieu.updatedUser = currentUser.Id;

            await _collection.InsertOneAsync(newDonOrder);
            var loaiDon = await _collectionLoaiDon.Find(x => x.Id == newDonOrder.loaiDon).FirstOrDefaultAsync();
            var donOrderRespond = _mapper.Map<DonOrderRespond>(newDonOrder);
            donOrderRespond.loaiDon = new IdName
            {
                Id = loaiDon.Id,
                Name = loaiDon.tenLoaiDon
            };
            return new RespondAPI<DonOrderRespond>(
                ResultRespond.Succeeded,
                "Tạo đơn order thành công.",
                donOrderRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<DonOrderRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi tạo đơn order: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<DonOrderRespond>> UpdateDonOrder(string id, RequestUpdateDonOrder request)
    {
        try
        {
            var filter = Builders<DonOrder>.Filter.Eq(x => x.Id, id);
            filter &= Builders<DonOrder>.Filter.Eq(x => x.isDelete, false);
            var donOrder = await _collection.Find(filter).FirstOrDefaultAsync();

            if (donOrder == null)
            {
                return new RespondAPI<DonOrderRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy đơn order với ID đã cung cấp."
                );
            }

            _mapper.Map(request, donOrder);

            donOrder.updatedDate = DateTimeOffset.UtcNow;

            // Cập nhật người dùng nếu có thông tin
            // danhMucNguyenLieu.updatedUser = currentUser.Id;

            var updateResult = await _collection.ReplaceOneAsync(filter, donOrder);

            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                return new RespondAPI<DonOrderRespond>(
                    ResultRespond.Error,
                    "Cập nhật đơn order không thành công."
                );
            }

            var donOrderRespond = _mapper.Map<DonOrderRespond>(donOrder);
            var loaiDon = await _collectionLoaiDon.Find(x => x.Id == donOrder.loaiDon).FirstOrDefaultAsync();
            donOrderRespond.loaiDon = new IdName
            {
                Id = loaiDon.Id,
                Name = loaiDon.tenLoaiDon
            };
            return new RespondAPI<DonOrderRespond>(
                ResultRespond.Succeeded,
                "Cập nhật đơn order thành công.",
                donOrderRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<DonOrderRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi cập nhật đơn order: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<string>> DeleteDonOrder(string id)
    {
        try
        {
            var existingDonOrder = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();
            if (existingDonOrder == null)
            {
                return new RespondAPI<string>(
                    ResultRespond.NotFound,
                    "Không tìm thấy đơn order để xóa."
                );
            }

            var deleteResult = await _collection.DeleteOneAsync(x => x.Id == id);

            if (deleteResult.DeletedCount == 0)
            {
                return new RespondAPI<string>(
                    ResultRespond.Error,
                    "Xóa đơn order không thành công."
                );
            }

            return new RespondAPI<string>(
                ResultRespond.Succeeded,
                "Xóa đơn order thành công.",
                id
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<string>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi xóa đơn order: {ex.Message}"
            );
        }
    }
}