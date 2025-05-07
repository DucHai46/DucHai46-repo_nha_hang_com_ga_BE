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
using repo_nha_hang_com_ga_BE.Models.Requests.MonAn;
using repo_nha_hang_com_ga_BE.Models.Responds.MonAn;

namespace repo_nha_hang_com_ga_BE.Repository.Imp;

public class MonAnRepository : IMonAnRepository
{
    private readonly IMongoCollection<MonAn> _collection;
    private readonly IMapper _mapper;
    private readonly IMongoCollection<LoaiMonAn> _collectionLoaiMonAn;
    private readonly IMongoCollection<CongThuc> _collectionCongThuc;
    private readonly IMongoCollection<GiamGia> _collectionGiamGia;


    public MonAnRepository(IOptions<MongoDbSettings> settings, IMapper mapper)
    {
        var mongoClientSettings = settings.Value;
        var client = new MongoClient(mongoClientSettings.Connection);
        var database = client.GetDatabase(mongoClientSettings.DatabaseName);
        _collection = database.GetCollection<MonAn>("MonAn");
        _collectionLoaiMonAn = database.GetCollection<LoaiMonAn>("LoaiMonAn");
        _collectionCongThuc = database.GetCollection<CongThuc>("CongThuc");
        _collectionGiamGia = database.GetCollection<GiamGia>("GiamGia");
        _mapper = mapper;
    }

    public async Task<RespondAPIPaging<List<MonAnRespond>>> GetAllMonAns(RequestSearchMonAn request)
    {
        try
        {
            var collection = _collection;

            var filter = Builders<MonAn>.Filter.Empty;
            filter &= Builders<MonAn>.Filter.Eq(x => x.isDelete, false);

            if (!string.IsNullOrEmpty(request.tenMonAn))
            {
                filter &= Builders<MonAn>.Filter.Regex(x => x.tenMonAn, new BsonRegularExpression($".*{request.tenMonAn}.*"));
            }

            if (!string.IsNullOrEmpty(request.idLoaiMonAn))
            {
                filter &= Builders<MonAn>.Filter.Eq(x => x.loaiMonAn, request.idLoaiMonAn);
            }

            if (!string.IsNullOrEmpty(request.idCongThuc))
            {
                filter &= Builders<MonAn>.Filter.Eq(x => x.congThuc, request.idCongThuc);
            }

            var projection = Builders<MonAn>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenMonAn)
                .Include(x => x.loaiMonAn)
                .Include(x => x.congThuc)
                .Include(x => x.giamGia)
                .Include(x => x.moTa)
                .Include(x => x.hinhAnh)
                .Include(x => x.giaTien);

            var findOptions = new FindOptions<MonAn, MonAn>
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
                var monAns = await cursor.ToListAsync();

                var loaiMonAnDict = new Dictionary<string, string>();

                var loaiMonAnIds = monAns.Select(x => x.loaiMonAn).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                var loaiMonAnFilter = Builders<LoaiMonAn>.Filter.In(x => x.Id, loaiMonAnIds);
                var loaiMonAnProjection = Builders<LoaiMonAn>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenLoai);
                var loaiMonAns = await _collectionLoaiMonAn.Find(loaiMonAnFilter)
                    .Project<LoaiMonAn>(loaiMonAnProjection)
                    .ToListAsync();

                loaiMonAnDict = loaiMonAns.ToDictionary(x => x.Id, x => x.tenLoai);

                var congThucDict = new Dictionary<string, string>();

                var congThucIds = monAns.Select(x => x.congThuc).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                var congThucFilter = Builders<CongThuc>.Filter.In(x => x.Id, congThucIds);
                var congThucProjection = Builders<CongThuc>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenCongThuc);
                var congThucs = await _collectionCongThuc.Find(congThucFilter)
                    .Project<CongThuc>(congThucProjection)
                    .ToListAsync();

                congThucDict = congThucs.ToDictionary(x => x.Id, x => x.tenCongThuc);

                var giamGiaDict = new Dictionary<string, string>();

                var giamGiaIds = monAns.Select(x => x.giamGia).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                var giamGiaFilter = Builders<GiamGia>.Filter.In(x => x.Id, giamGiaIds);
                var giamGiaProjection = Builders<GiamGia>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenGiamGia);
                var giamGias = await _collectionGiamGia.Find(giamGiaFilter)
                    .Project<GiamGia>(giamGiaProjection)
                    .ToListAsync();

                // giamGiaDict = giamGias.ToDictionary(x => x.Id, x => x.tenGiamGia);

                var monAnsRespond = monAns.Select(x => new MonAnRespond
                {
                    id = x.Id,
                    tenMonAn = x.tenMonAn,
                    loaiMonAn = new IdName
                    {
                        Id = x.loaiMonAn,
                        Name = loaiMonAnDict[x.loaiMonAn]
                    },
                    congThuc = new IdName
                    {
                        Id = x.congThuc,
                        Name = congThucDict[x.congThuc]
                    },
                    giamGia = new GiamGiaMonAnRespond
                    {
                        Id = x.giamGia,
                        Name = giamGias.FirstOrDefault(y => y.Id == x.giamGia)?.tenGiamGia,
                        giaTri = giamGias.FirstOrDefault(y => y.Id == x.giamGia)?.giaTri
                    },
                    moTa = x.moTa,
                    hinhAnh = x.hinhAnh,
                    giaTien = x.giaTien
                }).ToList();

                var pagingDetail = new PagingDetail(currentPage, request.PageSize, totalRecords);
                var pagingResponse = new PagingResponse<List<MonAnRespond>>
                {
                    Paging = pagingDetail,
                    Data = monAnsRespond
                };

                return new RespondAPIPaging<List<MonAnRespond>>(
                    ResultRespond.Succeeded,
                    data: pagingResponse
                );
            }
            else
            {
                var cursor = await collection.FindAsync(filter, findOptions);
                var monAns = await cursor.ToListAsync();

                var loaiMonAnDict = new Dictionary<string, string>();

                var loaiMonAnIds = monAns.Select(x => x.loaiMonAn).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                var loaiMonAnFilter = Builders<LoaiMonAn>.Filter.In(x => x.Id, loaiMonAnIds);
                var loaiMonAnProjection = Builders<LoaiMonAn>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenLoai);
                var loaiMonAns = await _collectionLoaiMonAn.Find(loaiMonAnFilter)
                    .Project<LoaiMonAn>(loaiMonAnProjection)
                    .ToListAsync();

                loaiMonAnDict = loaiMonAns.ToDictionary(x => x.Id, x => x.tenLoai);

                var congThucDict = new Dictionary<string, string>();

                var congThucIds = monAns.Select(x => x.congThuc).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                var congThucFilter = Builders<CongThuc>.Filter.In(x => x.Id, congThucIds);
                var congThucProjection = Builders<CongThuc>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenCongThuc);
                var congThucs = await _collectionCongThuc.Find(congThucFilter)
                    .Project<CongThuc>(congThucProjection)
                    .ToListAsync();

                congThucDict = congThucs.ToDictionary(x => x.Id, x => x.tenCongThuc);

                var giamGiaDict = new Dictionary<string, string>();

                var giamGiaIds = monAns.Select(x => x.giamGia).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                var giamGiaFilter = Builders<GiamGia>.Filter.In(x => x.Id, giamGiaIds);
                var giamGiaProjection = Builders<GiamGia>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenGiamGia);
                var giamGias = await _collectionGiamGia.Find(giamGiaFilter)
                    .Project<GiamGia>(giamGiaProjection)
                    .ToListAsync();

                // giamGiaDict = giamGias.ToDictionary(x => x.Id, x => x.tenGiamGia);

                var monAnsRespond = monAns.Select(x => new MonAnRespond
                {
                    id = x.Id,
                    tenMonAn = x.tenMonAn,
                    loaiMonAn = new IdName
                    {
                        Id = x.loaiMonAn,
                        Name = loaiMonAnDict[x.loaiMonAn]
                    },
                    congThuc = new IdName
                    {
                        Id = x.congThuc,
                        Name = congThucDict[x.congThuc]
                    },
                    giamGia = new GiamGiaMonAnRespond
                    {
                        Id = x.giamGia,
                        Name = giamGias.FirstOrDefault(y => y.Id == x.giamGia)?.tenGiamGia,
                        giaTri = giamGias.FirstOrDefault(y => y.Id == x.giamGia)?.giaTri
                    },
                    moTa = x.moTa,
                    hinhAnh = x.hinhAnh,
                    giaTien = x.giaTien
                }).ToList();

                return new RespondAPIPaging<List<MonAnRespond>>(
                    ResultRespond.Succeeded,
                    data: new PagingResponse<List<MonAnRespond>>
                    {
                        Data = monAnsRespond,
                        Paging = new PagingDetail(1, monAnsRespond.Count, monAnsRespond.Count)
                    }
                );
            }
        }
        catch (Exception ex)
        {
            return new RespondAPIPaging<List<MonAnRespond>>(
                ResultRespond.Error,
                message: ex.Message
            );
        }
    }

    public async Task<RespondAPI<MonAnRespond>> GetMonAnById(string id)
    {
        try
        {
            var monAn = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();

            if (monAn == null)
            {
                return new RespondAPI<MonAnRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy món ăn với ID đã cung cấp."
                );
            }

            // var monAnRespond = _mapper.Map<MonAnRespond>(monAn);
            var loaiMonAn = await _collectionLoaiMonAn.Find(x => x.Id == monAn.loaiMonAn).FirstOrDefaultAsync();
            var congThuc = await _collectionCongThuc.Find(x => x.Id == monAn.congThuc).FirstOrDefaultAsync();
            var giamGia = await _collectionGiamGia.Find(x => x.Id == monAn.giamGia).FirstOrDefaultAsync();

            var monAnRespond = new MonAnRespond
            {
                id = monAn.Id,
                tenMonAn = monAn.tenMonAn,
                loaiMonAn = new IdName
                {
                    Id = loaiMonAn.Id,
                    Name = loaiMonAn.tenLoai
                },
                congThuc = new IdName
                {
                    Id = congThuc.Id,
                    Name = congThuc.tenCongThuc
                },
                giamGia = new GiamGiaMonAnRespond
                {
                    Id = giamGia.Id,
                    Name = giamGia.tenGiamGia,
                    giaTri = giamGia.giaTri
                },
                moTa = monAn.moTa,
                hinhAnh = monAn.hinhAnh,
                giaTien = monAn.giaTien
            };

            return new RespondAPI<MonAnRespond>(
                ResultRespond.Succeeded,
                "Lấy món ăn thành công.",
                monAnRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<MonAnRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<MonAnRespond>> CreateMonAn(RequestAddMonAn request)
    {
        try
        {
            MonAn newMonAn = _mapper.Map<MonAn>(request);

            newMonAn.createdDate = DateTimeOffset.UtcNow;
            newMonAn.updatedDate = DateTimeOffset.UtcNow;
            newMonAn.isDelete = false;
            // Thiết lập createdUser và updatedUser nếu có thông tin người dùng
            // newLoaiNguyenLieu.createdUser = currentUser.Id;
            // newDanhMucNguyenLieu.updatedUser = currentUser.Id;

            await _collection.InsertOneAsync(newMonAn);

            var loaiMonAn = await _collectionLoaiMonAn.Find(x => x.Id == newMonAn.loaiMonAn).FirstOrDefaultAsync();
            var congThuc = await _collectionCongThuc.Find(x => x.Id == newMonAn.congThuc).FirstOrDefaultAsync();
            var giamGia = await _collectionGiamGia.Find(x => x.Id == newMonAn.giamGia).FirstOrDefaultAsync();

            var monAnRespond = new MonAnRespond
            {
                id = newMonAn.Id,
                tenMonAn = newMonAn.tenMonAn,
                loaiMonAn = new IdName
                {
                    Id = loaiMonAn.Id,
                    Name = loaiMonAn.tenLoai
                },
                congThuc = new IdName
                {
                    Id = congThuc.Id,
                    Name = congThuc.tenCongThuc
                },
                giamGia = new GiamGiaMonAnRespond
                {
                    Id = giamGia.Id,
                    Name = giamGia.tenGiamGia,
                    giaTri = giamGia.giaTri
                },
                moTa = newMonAn.moTa,
                hinhAnh = newMonAn.hinhAnh,
                giaTien = newMonAn.giaTien
            };

            // var monAnRespond = _mapper.Map<MonAnRespond>(newMonAn);

            return new RespondAPI<MonAnRespond>(
                ResultRespond.Succeeded,
                "Tạo món ăn thành công.",
                monAnRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<MonAnRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi tạo món ăn: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<MonAnRespond>> UpdateMonAn(string id, RequestUpdateMonAn request)
    {
        try
        {
            var filter = Builders<MonAn>.Filter.Eq(x => x.Id, id);
            filter &= Builders<MonAn>.Filter.Eq(x => x.isDelete, false);
            var monAn = await _collection.Find(filter).FirstOrDefaultAsync();

            if (monAn == null)
            {
                return new RespondAPI<MonAnRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy món ăn với ID đã cung cấp."
                );
            }

            _mapper.Map(request, monAn);

            monAn.updatedDate = DateTimeOffset.UtcNow;

            // Cập nhật người dùng nếu có thông tin
            // danhMucNguyenLieu.updatedUser = currentUser.Id;

            var updateResult = await _collection.ReplaceOneAsync(filter, monAn);

            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                return new RespondAPI<MonAnRespond>(
                    ResultRespond.Error,
                    "Cập nhật món ăn không thành công."
                );
            }

            // var monAnRespond = _mapper.Map<MonAnRespond>(monAn);
            var loaiMonAn = await _collectionLoaiMonAn.Find(x => x.Id == monAn.loaiMonAn).FirstOrDefaultAsync();
            var congThuc = await _collectionCongThuc.Find(x => x.Id == monAn.congThuc).FirstOrDefaultAsync();
            var giamGia = await _collectionGiamGia.Find(x => x.Id == monAn.giamGia).FirstOrDefaultAsync();

            var monAnRespond = new MonAnRespond
            {
                id = monAn.Id,
                tenMonAn = monAn.tenMonAn,
                loaiMonAn = new IdName
                {
                    Id = loaiMonAn.Id,
                    Name = loaiMonAn.tenLoai
                },
                congThuc = new IdName
                {
                    Id = congThuc.Id,
                    Name = congThuc.tenCongThuc
                },
                giamGia = new GiamGiaMonAnRespond
                {
                    Id = giamGia.Id,
                    Name = giamGia.tenGiamGia,
                    giaTri = giamGia.giaTri
                },
                moTa = monAn.moTa,
                hinhAnh = monAn.hinhAnh,
                giaTien = monAn.giaTien
            };

            return new RespondAPI<MonAnRespond>(
                ResultRespond.Succeeded,
                "Cập nhật món ăn thành công.",
                monAnRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<MonAnRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi cập nhật món ăn: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<string>> DeleteMonAn(string id)
    {
        try
        {
            var existingMonAn = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();
            if (existingMonAn == null)
            {
                return new RespondAPI<string>(
                    ResultRespond.NotFound,
                    "Không tìm thấy món ăn để xóa."
                );
            }

            var deleteResult = await _collection.DeleteOneAsync(x => x.Id == id);

            if (deleteResult.DeletedCount == 0)
            {
                return new RespondAPI<string>(
                    ResultRespond.Error,
                    "Xóa món ăn không thành công."
                );
            }

            return new RespondAPI<string>(
                ResultRespond.Succeeded,
                "Xóa món ăn thành công.",
                id
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<string>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi xóa món ăn: {ex.Message}"
            );
        }
    }
}