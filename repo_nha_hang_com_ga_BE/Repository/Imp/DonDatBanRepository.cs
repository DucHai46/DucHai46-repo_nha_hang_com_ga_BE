
using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models.Respond;
using repo_nha_hang_com_ga_BE.Models.Common.Respond;
using repo_nha_hang_com_ga_BE.Models.MongoDB;
using repo_nha_hang_com_ga_BE.Models.Requests.DonDatBan;
using repo_nha_hang_com_ga_BE.Models.Responds.DonDatBan;
using repo_nha_hang_com_ga_BE.Models.Common.Models;
using repo_nha_hang_com_ga_BE.Models.Common.Paging;


namespace repo_nha_hang_com_ga_BE.Repository.Imp;

public class DonDatBanRepository : IDonDatBanRepository
{
    private readonly IMongoCollection<DonDatBan> _collection;
    private readonly IMongoCollection<KhachHang> _collectionkhachHang;
    private readonly IMongoCollection<LoaiBan> _collectionLoaiBan;
    private readonly IMapper _mapper;

    public DonDatBanRepository(IOptions<MongoDbSettings> settings, IMapper mapper)
    {
        var mongoClientSettings = settings.Value;
        var client = new MongoClient(mongoClientSettings.Connection);
        var database = client.GetDatabase(mongoClientSettings.DatabaseName);
        _collection = database.GetCollection<DonDatBan>("DonDatBan");
        _collectionkhachHang = database.GetCollection<KhachHang>("KhachHang");
        _collectionLoaiBan = database.GetCollection<LoaiBan>("LoaiBan");
        _mapper = mapper;
    }

    public async Task<RespondAPIPaging<List<DonDatBanRespond>>> GetAllDonDatBan(RequestSearchDonDatBan request)
    {
        try
        {
            var collection = _collection;

            var filter = Builders<DonDatBan>.Filter.Empty;
            filter &= Builders<DonDatBan>.Filter.Eq(x => x.isDelete, false);

            if (!string.IsNullOrEmpty(request.ban))
            {
                filter &= Builders<DonDatBan>.Filter.Eq(x => x.ban, request.ban);
            }

            if (!string.IsNullOrEmpty(request.khachHang))
            {
                filter &= Builders<DonDatBan>.Filter.Regex(x => x.khachHang, request.khachHang);
                // filter &= Builders<DonDatBan>.Filter.Regex(x => x.khachHang!.Name, new BsonRegularExpression($".*{request.khachHangName}.*"));
            }

            if (!string.IsNullOrEmpty(request.khungGio))
            {
                filter &= Builders<DonDatBan>.Filter.Regex(x => x.khungGio, request.khungGio);
            }

            // if (request.ban != null)
            // {
            //     filter &= Builders<DonDatBan>.Filter.Eq(x => x.ban, request.ban);
            // }

            var projection = Builders<DonDatBan>.Projection
                .Include(x => x.Id)
                .Include(x => x.ban)
                .Include(x => x.khachHang)
                .Include(x => x.khungGio);

            var findOptions = new FindOptions<DonDatBan, DonDatBan>
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
                var DonDatBans = await cursor.ToListAsync();

                var loaiBanIds = DonDatBans.Select(x => x.ban).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                var khachHangIds = DonDatBans.Select(x => x.khachHang).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                var loaiBanFilter = Builders<LoaiBan>.Filter.In(x => x.Id, loaiBanIds);
                var loaiBanProjection = Builders<LoaiBan>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenLoai);
                var loaiBans = await _collectionLoaiBan.Find(loaiBanFilter)
                    .Project<LoaiBan>(loaiBanProjection)
                    .ToListAsync();

                var khachHangFilter = Builders<KhachHang>.Filter.In(x => x.Id, khachHangIds);
                var khachHangProjection = Builders<KhachHang>.Projection
                   .Include(x => x.Id)
                   .Include(x => x.tenKhachHang);
                var khachHangs = await _collectionkhachHang.Find(khachHangFilter)
                  .Project<KhachHang>(khachHangProjection)
                  .ToListAsync();

                var loaiBanDict = loaiBans.ToDictionary(x => x.Id, x => x.tenLoai);
                var khachHangDict = khachHangs.ToDictionary(x => x.Id, x => x.tenKhachHang);

                var donDatBanRespond = DonDatBans.Select(donDatBan => new DonDatBanRespond
                {
                    id = donDatBan.Id,
                    ban = new IdName
                    {
                        Id = donDatBan.ban,
                        Name = loaiBanDict.ContainsKey(donDatBan.ban) ? loaiBanDict[donDatBan.ban] : null
                    },
                    khachHang = new IdName
                    {
                        Id = donDatBan.khachHang,
                        Name = khachHangDict.ContainsKey(donDatBan.khachHang) ? khachHangDict[donDatBan.khachHang] : null
                    },
                    khungGio = donDatBan.khungGio
                }).ToList();


                var pagingDetail = new PagingDetail(currentPage, request.PageSize, totalRecords);
                var pagingResponse = new PagingResponse<List<DonDatBanRespond>>
                {
                    Paging = pagingDetail,
                    Data = donDatBanRespond
                };

                return new RespondAPIPaging<List<DonDatBanRespond>>(
                    ResultRespond.Succeeded,
                    data: pagingResponse
                );
            }
            else
            {
                var cursor = await collection.FindAsync(filter, findOptions);
                var DonDatBans = await cursor.ToListAsync();

                var loaiBanIds = DonDatBans.Select(x => x.ban).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                var khachHangIds = DonDatBans.Select(x => x.khachHang).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                var loaiBanFilter = Builders<LoaiBan>.Filter.In(x => x.Id, loaiBanIds);
                var loaiBanProjection = Builders<LoaiBan>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenLoai);
                var loaiBans = await _collectionLoaiBan.Find(loaiBanFilter)
                    .Project<LoaiBan>(loaiBanProjection)
                    .ToListAsync();

                var khachHangFilter = Builders<KhachHang>.Filter.In(x => x.Id, khachHangIds);
                var khachHangProjection = Builders<KhachHang>.Projection
                   .Include(x => x.Id)
                   .Include(x => x.tenKhachHang);
                var khachHangs = await _collectionkhachHang.Find(khachHangFilter)
                  .Project<KhachHang>(khachHangProjection)
                  .ToListAsync();

                var loaiBanDict = loaiBans.ToDictionary(x => x.Id, x => x.tenLoai);
                var khachHangDict = khachHangs.ToDictionary(x => x.Id, x => x.tenKhachHang);

                var donDatBanRespond = DonDatBans.Select(donDatBan => new DonDatBanRespond
                {
                    id = donDatBan.Id,
                    ban = new IdName
                    {
                        Id = donDatBan.ban,
                        Name = loaiBanDict.ContainsKey(donDatBan.ban) ? loaiBanDict[donDatBan.ban] : null
                    },
                    khachHang = new IdName
                    {
                        Id = donDatBan.khachHang,
                        Name = khachHangDict.ContainsKey(donDatBan.khachHang) ? khachHangDict[donDatBan.khachHang] : null
                    },
                    khungGio = donDatBan.khungGio
                }).ToList();

                return new RespondAPIPaging<List<DonDatBanRespond>>(
                    ResultRespond.Succeeded,
                    data: new PagingResponse<List<DonDatBanRespond>>
                    {
                        Data = donDatBanRespond,
                        Paging = new PagingDetail(1, DonDatBans.Count, DonDatBans.Count)
                    }
                );
            }
        }
        catch (Exception ex)
        {
            return new RespondAPIPaging<List<DonDatBanRespond>>(
                ResultRespond.Error,
                message: ex.Message
            );
        }
    }

    public async Task<RespondAPI<DonDatBanRespond>> GetDonDatBanById(string id)
    {
        try
        {
            var donDatBan = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();

            if (donDatBan == null)
            {
                return new RespondAPI<DonDatBanRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy đơn đặt bàn với ID đã cung cấp."
                );
            }

            // var donDatBanRespond = _mapper.Map<DonDatBanRespond>(donDatBan);

            var loaiBanIds = new List<string> { donDatBan.ban }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var khachHangIds = new List<string> { donDatBan.khachHang }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

            var loaiBanFilter = Builders<LoaiBan>.Filter.In(x => x.Id, loaiBanIds);
            var loaiBanProjection = Builders<LoaiBan>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenLoai);
            var loaiBans = await _collectionLoaiBan.Find(loaiBanFilter)
                .Project<LoaiBan>(loaiBanProjection)
                .ToListAsync();

            var khachHangFilter = Builders<KhachHang>.Filter.In(x => x.Id, khachHangIds);
            var khachHangProjection = Builders<KhachHang>.Projection
               .Include(x => x.Id)
               .Include(x => x.tenKhachHang);
            var khachHangs = await _collectionkhachHang.Find(khachHangFilter)
              .Project<KhachHang>(khachHangProjection)
              .ToListAsync();

            var loaiBanDict = loaiBans.ToDictionary(x => x.Id, x => x.tenLoai);
            var khachHangDict = khachHangs.ToDictionary(x => x.Id, x => x.tenKhachHang);

            var donDatBanRespond = new DonDatBanRespond
            {
                id = donDatBan.Id,
                ban = new IdName
                {
                    Id = donDatBan.ban,
                    Name = loaiBanDict.ContainsKey(donDatBan.ban) ? loaiBanDict[donDatBan.ban] : null
                },
                khachHang = new IdName
                {
                    Id = donDatBan.khachHang,
                    Name = khachHangDict.ContainsKey(donDatBan.khachHang) ? khachHangDict[donDatBan.khachHang] : null
                },
                khungGio = donDatBan.khungGio
            };

            return new RespondAPI<DonDatBanRespond>(
                ResultRespond.Succeeded,
                "Lấy đơn đặt bàn thành công.",
                donDatBanRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<DonDatBanRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<DonDatBanRespond>> CreateDonDatBan(RequestAddDonDatBan request)
    {
        try
        {
            DonDatBan newDonDatBan = _mapper.Map<DonDatBan>(request);

            newDonDatBan.createdDate = DateTimeOffset.UtcNow;
            newDonDatBan.updatedDate = DateTimeOffset.UtcNow;
            newDonDatBan.isDelete = false;
            // Thiết lập createdUser và updatedUser nếu có thông tin người dùng
            // newDanhMucMonAn.createdUser = currentUser.Id;
            // newDanhMucNguyenLieu.updatedUser = currentUser.Id;

            await _collection.InsertOneAsync(newDonDatBan);

            // var donDatBanRespond = _mapper.Map<DonDatBanRespond>(newDonDatBan);


            var loaiBanIds = new List<string> { newDonDatBan.ban }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var khachHangIds = new List<string> { newDonDatBan.khachHang }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

            var loaiBanFilter = Builders<LoaiBan>.Filter.In(x => x.Id, loaiBanIds);
            var loaiBanProjection = Builders<LoaiBan>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenLoai);
            var loaiBans = await _collectionLoaiBan.Find(loaiBanFilter)
                .Project<LoaiBan>(loaiBanProjection)
                .ToListAsync();

            var khachHangFilter = Builders<KhachHang>.Filter.In(x => x.Id, khachHangIds);
            var khachHangProjection = Builders<KhachHang>.Projection
               .Include(x => x.Id)
               .Include(x => x.tenKhachHang);
            var khachHangs = await _collectionkhachHang.Find(khachHangFilter)
              .Project<KhachHang>(khachHangProjection)
              .ToListAsync();

            var loaiBanDict = loaiBans.ToDictionary(x => x.Id, x => x.tenLoai);
            var khachHangDict = khachHangs.ToDictionary(x => x.Id, x => x.tenKhachHang);

            var donDatBanRespond = new DonDatBanRespond
            {
                id = newDonDatBan.Id,
                ban = new IdName
                {
                    Id = newDonDatBan.ban,
                    Name = loaiBanDict.ContainsKey(newDonDatBan.ban) ? loaiBanDict[newDonDatBan.ban] : null
                },
                khachHang = new IdName
                {
                    Id = newDonDatBan.khachHang,
                    Name = khachHangDict.ContainsKey(newDonDatBan.khachHang) ? khachHangDict[newDonDatBan.khachHang] : null
                },
                khungGio = newDonDatBan.khungGio
            };

            return new RespondAPI<DonDatBanRespond>(
                ResultRespond.Succeeded,
                "Tạo đơn đặt bàn thành công.",
                donDatBanRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<DonDatBanRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi tạo đơn đặt bàn: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<DonDatBanRespond>> UpdateDonDatBan(string id, RequestUpdateDonDatBan request)
    {
        try
        {
            var filter = Builders<DonDatBan>.Filter.Eq(x => x.Id, id);
            filter &= Builders<DonDatBan>.Filter.Eq(x => x.isDelete, false);
            var donDatBan = await _collection.Find(filter).FirstOrDefaultAsync();

            if (donDatBan == null)
            {
                return new RespondAPI<DonDatBanRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy đơn đặt bàn với ID đã cung cấp."
                );
            }

            _mapper.Map(request, donDatBan);

            donDatBan.updatedDate = DateTimeOffset.UtcNow;

            // Cập nhật người dùng nếu có thông tin
            // danhMucNguyenLieu.updatedUser = currentUser.Id;

            var updateResult = await _collection.ReplaceOneAsync(filter, donDatBan);

            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                return new RespondAPI<DonDatBanRespond>(
                    ResultRespond.Error,
                    "Cập nhật đơn đặt bàn không thành công."
                );
            }

            // var donDatBanRespond = _mapper.Map<DonDatBanRespond>(donDatBan);
            var loaiBanIds = new List<string> { donDatBan.ban }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var khachHangIds = new List<string> { donDatBan.khachHang }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

            var loaiBanFilter = Builders<LoaiBan>.Filter.In(x => x.Id, loaiBanIds);
            var loaiBanProjection = Builders<LoaiBan>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenLoai);
            var loaiBans = await _collectionLoaiBan.Find(loaiBanFilter)
                .Project<LoaiBan>(loaiBanProjection)
                .ToListAsync();

            var khachHangFilter = Builders<KhachHang>.Filter.In(x => x.Id, khachHangIds);
            var khachHangProjection = Builders<KhachHang>.Projection
               .Include(x => x.Id)
               .Include(x => x.tenKhachHang);
            var khachHangs = await _collectionkhachHang.Find(khachHangFilter)
              .Project<KhachHang>(khachHangProjection)
              .ToListAsync();

            var loaiBanDict = loaiBans.ToDictionary(x => x.Id, x => x.tenLoai);
            var khachHangDict = khachHangs.ToDictionary(x => x.Id, x => x.tenKhachHang);

            var donDatBanRespond = new DonDatBanRespond
            {
                id = donDatBan.Id,
                ban = new IdName
                {
                    Id = donDatBan.ban,
                    Name = loaiBanDict.ContainsKey(donDatBan.ban) ? loaiBanDict[donDatBan.ban] : null
                },
                khachHang = new IdName
                {
                    Id = donDatBan.khachHang,
                    Name = khachHangDict.ContainsKey(donDatBan.khachHang) ? khachHangDict[donDatBan.khachHang] : null
                },
                khungGio = donDatBan.khungGio
            };

            return new RespondAPI<DonDatBanRespond>(
                ResultRespond.Succeeded,
                "Cập nhật đơn đặt bàn thành công.",
                donDatBanRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<DonDatBanRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi cập nhật đơn đặt bàn: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<string>> DeleteDonDatBan(string id)
    {
        try
        {
            var existingDonDatBan = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();
            if (existingDonDatBan == null)
            {
                return new RespondAPI<string>(
                    ResultRespond.NotFound,
                    "Không tìm thấy đơn đặt bàn để xóa."
                );
            }

            var deleteResult = await _collection.DeleteOneAsync(x => x.Id == id);

            if (deleteResult.DeletedCount == 0)
            {
                return new RespondAPI<string>(
                    ResultRespond.Error,
                    "Xóa đơn đặt bàn không thành công."
                );
            }

            return new RespondAPI<string>(
                ResultRespond.Succeeded,
                "Xóa đơn đặt bàn thành công.",
                id
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<string>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi xóa đơn đặt bàn: {ex.Message}"
            );
        }
    }
}