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
using repo_nha_hang_com_ga_BE.Models.Requests.NguyenLieu;
using repo_nha_hang_com_ga_BE.Models.Responds.NguyenLieu;

namespace repo_nha_hang_com_ga_BE.Repository.Imp;

public class NguyenLieuRepository : INguyenLieuRepository
{
    private readonly IMongoCollection<NguyenLieu> _collection;
    private readonly IMongoCollection<LoaiNguyenLieu> _collectionLoaiNguyenLieu;
    private readonly IMongoCollection<DonViTinh> _collectionDonViTinh;
    private readonly IMongoCollection<TuDo> _collectionTuDo;
    private readonly IMapper _mapper;

    public NguyenLieuRepository(IOptions<MongoDbSettings> settings, IMapper mapper)
    {
        var mongoClientSettings = settings.Value;
        var client = new MongoClient(mongoClientSettings.Connection);
        var database = client.GetDatabase(mongoClientSettings.DatabaseName);
        _collection = database.GetCollection<NguyenLieu>("NguyenLieu");
        _collectionLoaiNguyenLieu = database.GetCollection<LoaiNguyenLieu>("LoaiNguyenLieu");
        _collectionDonViTinh = database.GetCollection<DonViTinh>("DonViTinh");
        _collectionTuDo = database.GetCollection<TuDo>("TuDo");
        _mapper = mapper;
    }

    public async Task<RespondAPIPaging<List<NguyenLieuRespond>>> GetAllNguyenLieus(RequestSearchNguyenLieu request)
    {
        try
        {
            var collection = _collection;

            var filter = Builders<NguyenLieu>.Filter.Empty;
            filter &= Builders<NguyenLieu>.Filter.Eq(x => x.isDelete, false);

            if (!string.IsNullOrEmpty(request.loaiNguyenLieuId))
            {
                filter &= Builders<NguyenLieu>.Filter.Eq(x => x.loaiNguyenLieu, request.loaiNguyenLieuId);
            }

            if (!string.IsNullOrEmpty(request.donViTinhId))
            {
                filter &= Builders<NguyenLieu>.Filter.Eq(x => x.donViTinh, request.donViTinhId);
            }

            if (!string.IsNullOrEmpty(request.tenNguyenLieu))
            {
                filter &= Builders<NguyenLieu>.Filter.Regex(x => x.tenNguyenLieu, new BsonRegularExpression($".*{request.tenNguyenLieu}.*"));
            }

            // if (request.hanSuDung != null)
            // {
            //     filter &= Builders<NguyenLieu>.Filter.Gte(x => x.hanSuDung, request.hanSuDung);
            // }

            if (!string.IsNullOrEmpty(request.tuDoId))
            {
                filter &= Builders<NguyenLieu>.Filter.Eq(x => x.tuDo, request.tuDoId);
            }

            if (request.trangThai != null)
            {
                filter &= Builders<NguyenLieu>.Filter.Eq(x => x.trangThai, request.trangThai);
            }

            var projection = Builders<NguyenLieu>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenNguyenLieu)
                .Include(x => x.moTa)
                .Include(x => x.soLuong)
                .Include(x => x.trangThai)
                // .Include(x => x.hanSuDung)
                .Include(x => x.loaiNguyenLieu)
                .Include(x => x.donViTinh)
                .Include(x => x.tuDo);

            var findOptions = new FindOptions<NguyenLieu, NguyenLieu>
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
                var nguyenLieus = await cursor.ToListAsync();

                // Lấy danh sách ID loại bàn
                var loaiNguyenLieuIds = nguyenLieus.Select(x => x.loaiNguyenLieu).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                // Query bảng loại bàn
                var loaiNguyenLieuFilter = Builders<LoaiNguyenLieu>.Filter.In(x => x.Id, loaiNguyenLieuIds);
                var loaiNguyenLieuProjection = Builders<LoaiNguyenLieu>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenLoai);
                var loaiNguyenLieus = await _collectionLoaiNguyenLieu.Find(loaiNguyenLieuFilter)
                    .Project<LoaiNguyenLieu>(loaiNguyenLieuProjection)
                    .ToListAsync();

                // Tạo dictionary để map nhanh
                var loaiNguyenLieuDict = loaiNguyenLieus.ToDictionary(x => x.Id, x => x.tenLoai);

                var donViTinhIds = nguyenLieus.Select(x => x.donViTinh).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                var donViTinhFilter = Builders<DonViTinh>.Filter.In(x => x.Id, donViTinhIds);
                var donViTinhProjection = Builders<DonViTinh>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenDonViTinh);
                var donViTinhs = await _collectionDonViTinh.Find(donViTinhFilter)
                    .Project<DonViTinh>(donViTinhProjection)
                    .ToListAsync();

                var donViTinhDict = donViTinhs.ToDictionary(x => x.Id, x => x.tenDonViTinh);

                var tuDoIds = nguyenLieus.Select(x => x.tuDo).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                var tuDoFilter = Builders<TuDo>.Filter.In(x => x.Id, tuDoIds);
                var tuDoProjection = Builders<TuDo>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenTuDo);
                var tuDos = await _collectionTuDo.Find(tuDoFilter)
                    .Project<TuDo>(tuDoProjection)
                    .ToListAsync();

                var tuDoDict = tuDos.ToDictionary(x => x.Id, x => x.tenTuDo);

                var nguyenLieuResponds = nguyenLieus.Select(nguyenLieu => new NguyenLieuRespond
                {
                    id = nguyenLieu.Id,
                    tenNguyenLieu = nguyenLieu.tenNguyenLieu,
                    moTa = nguyenLieu.moTa,
                    soLuong = nguyenLieu.soLuong,
                    trangThai = nguyenLieu.trangThai,
                    loaiNguyenLieu = new IdName
                    {
                        Id = nguyenLieu.loaiNguyenLieu,
                        Name = loaiNguyenLieuDict.ContainsKey(nguyenLieu.loaiNguyenLieu) ? loaiNguyenLieuDict[nguyenLieu.loaiNguyenLieu] : null
                    },
                    donViTinh = new IdName
                    {
                        Id = nguyenLieu.donViTinh,
                        Name = donViTinhDict.ContainsKey(nguyenLieu.donViTinh) ? donViTinhDict[nguyenLieu.donViTinh] : null
                    },
                    tuDo = new IdName
                    {
                        Id = nguyenLieu.tuDo,
                        Name = tuDoDict.ContainsKey(nguyenLieu.tuDo) ? tuDoDict[nguyenLieu.tuDo] : null
                    }
                }).ToList();


                var pagingDetail = new PagingDetail(currentPage, request.PageSize, totalRecords);
                var pagingResponse = new PagingResponse<List<NguyenLieuRespond>>
                {
                    Paging = pagingDetail,
                    Data = nguyenLieuResponds
                };

                return new RespondAPIPaging<List<NguyenLieuRespond>>(
                    ResultRespond.Succeeded,
                    data: pagingResponse
                );
            }
            else
            {
                var cursor = await collection.FindAsync(filter, findOptions);
                var nguyenLieus = await cursor.ToListAsync();

                // Lấy danh sách ID loại bàn
                var loaiNguyenLieuIds = nguyenLieus.Select(x => x.loaiNguyenLieu).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                // Query bảng loại bàn
                var loaiNguyenLieuFilter = Builders<LoaiNguyenLieu>.Filter.In(x => x.Id, loaiNguyenLieuIds);
                var loaiNguyenLieuProjection = Builders<LoaiNguyenLieu>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenLoai);
                var loaiNguyenLieus = await _collectionLoaiNguyenLieu.Find(loaiNguyenLieuFilter)
                    .Project<LoaiNguyenLieu>(loaiNguyenLieuProjection)
                    .ToListAsync();

                // Tạo dictionary để map nhanh
                var loaiNguyenLieuDict = loaiNguyenLieus.ToDictionary(x => x.Id, x => x.tenLoai);

                var donViTinhIds = nguyenLieus.Select(x => x.donViTinh).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                var donViTinhFilter = Builders<DonViTinh>.Filter.In(x => x.Id, donViTinhIds);
                var donViTinhProjection = Builders<DonViTinh>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenDonViTinh);
                var donViTinhs = await _collectionDonViTinh.Find(donViTinhFilter)
                    .Project<DonViTinh>(donViTinhProjection)
                    .ToListAsync();

                var donViTinhDict = donViTinhs.ToDictionary(x => x.Id, x => x.tenDonViTinh);

                var tuDoIds = nguyenLieus.Select(x => x.tuDo).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                var tuDoFilter = Builders<TuDo>.Filter.In(x => x.Id, tuDoIds);
                var tuDoProjection = Builders<TuDo>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenTuDo);
                var tuDos = await _collectionTuDo.Find(tuDoFilter)
                    .Project<TuDo>(tuDoProjection)
                    .ToListAsync();

                var tuDoDict = tuDos.ToDictionary(x => x.Id, x => x.tenTuDo);

                var nguyenLieuResponds = nguyenLieus.Select(nguyenLieu => new NguyenLieuRespond
                {
                    id = nguyenLieu.Id,
                    tenNguyenLieu = nguyenLieu.tenNguyenLieu,
                    moTa = nguyenLieu.moTa,
                    soLuong = nguyenLieu.soLuong,
                    trangThai = nguyenLieu.trangThai,
                    loaiNguyenLieu = new IdName
                    {
                        Id = nguyenLieu.loaiNguyenLieu,
                        Name = loaiNguyenLieuDict.ContainsKey(nguyenLieu.loaiNguyenLieu) ? loaiNguyenLieuDict[nguyenLieu.loaiNguyenLieu] : null
                    },
                    donViTinh = new IdName
                    {
                        Id = nguyenLieu.donViTinh,
                        Name = donViTinhDict.ContainsKey(nguyenLieu.donViTinh) ? donViTinhDict[nguyenLieu.donViTinh] : null
                    },
                    tuDo = new IdName
                    {
                        Id = nguyenLieu.tuDo,
                        Name = tuDoDict.ContainsKey(nguyenLieu.tuDo) ? tuDoDict[nguyenLieu.tuDo] : null
                    }
                }).ToList();

                return new RespondAPIPaging<List<NguyenLieuRespond>>(
                    ResultRespond.Succeeded,
                    data: new PagingResponse<List<NguyenLieuRespond>>
                    {
                        Data = nguyenLieuResponds,
                        Paging = new PagingDetail(1, nguyenLieuResponds.Count, nguyenLieuResponds.Count)
                    }
                );
            }
        }
        catch (Exception ex)
        {
            return new RespondAPIPaging<List<NguyenLieuRespond>>(
                ResultRespond.Error,
                message: ex.Message
            );
        }
    }

    public async Task<RespondAPI<NguyenLieuRespond>> GetNguyenLieuById(string id)
    {
        try
        {
            var nguyenLieu = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();

            if (nguyenLieu == null)
            {
                return new RespondAPI<NguyenLieuRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy nguyên liệu với ID đã cung cấp."
                );
            }

            var loaiNguyenLieu = await _collectionLoaiNguyenLieu.Find(x => x.Id == nguyenLieu.loaiNguyenLieu).FirstOrDefaultAsync();
            var donViTinh = await _collectionDonViTinh.Find(x => x.Id == nguyenLieu.donViTinh).FirstOrDefaultAsync();
            var tuDo = await _collectionTuDo.Find(x => x.Id == nguyenLieu.tuDo).FirstOrDefaultAsync();

            var nguyenLieuRespond = new NguyenLieuRespond
            {
                id = nguyenLieu.Id,
                tenNguyenLieu = nguyenLieu.tenNguyenLieu,
                moTa = nguyenLieu.moTa,
                soLuong = nguyenLieu.soLuong,
                trangThai = nguyenLieu.trangThai,
                loaiNguyenLieu = new IdName
                {
                    Id = loaiNguyenLieu.Id,
                    Name = loaiNguyenLieu.tenLoai
                },
                donViTinh = new IdName
                {
                    Id = donViTinh.Id,
                    Name = donViTinh.tenDonViTinh
                },
                tuDo = new IdName
                {
                    Id = tuDo.Id,
                    Name = tuDo.tenTuDo
                }
            };

            return new RespondAPI<NguyenLieuRespond>(
                ResultRespond.Succeeded,
                "Lấy nguyên liệu thành công.",
                nguyenLieuRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<NguyenLieuRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<NguyenLieuRespond>> CreateNguyenLieu(RequestAddNguyenLieu request)
    {
        try
        {
            NguyenLieu newNguyenLieu = _mapper.Map<NguyenLieu>(request);

            newNguyenLieu.createdDate = DateTimeOffset.UtcNow;
            newNguyenLieu.updatedDate = DateTimeOffset.UtcNow;
            newNguyenLieu.isDelete = false;
            // Thiết lập createdUser và updatedUser nếu có thông tin người dùng
            // newLoaiNguyenLieu.createdUser = currentUser.Id;
            // newDanhMucNguyenLieu.updatedUser = currentUser.Id;

            await _collection.InsertOneAsync(newNguyenLieu);

            var loaiNguyenLieu = await _collectionLoaiNguyenLieu.Find(x => x.Id == newNguyenLieu.loaiNguyenLieu).FirstOrDefaultAsync();
            var donViTinh = await _collectionDonViTinh.Find(x => x.Id == newNguyenLieu.donViTinh).FirstOrDefaultAsync();
            var tuDo = await _collectionTuDo.Find(x => x.Id == newNguyenLieu.tuDo).FirstOrDefaultAsync();

            var nguyenLieuRespond = new NguyenLieuRespond
            {
                id = newNguyenLieu.Id,
                tenNguyenLieu = newNguyenLieu.tenNguyenLieu,
                moTa = newNguyenLieu.moTa,
                soLuong = newNguyenLieu.soLuong,
                trangThai = newNguyenLieu.trangThai,
                loaiNguyenLieu = new IdName
                {
                    Id = loaiNguyenLieu.Id,
                    Name = loaiNguyenLieu.tenLoai
                },
                donViTinh = new IdName
                {
                    Id = donViTinh.Id,
                    Name = donViTinh.tenDonViTinh
                },
                tuDo = new IdName
                {
                    Id = tuDo.Id,
                    Name = tuDo.tenTuDo
                }
            };

            return new RespondAPI<NguyenLieuRespond>(
                ResultRespond.Succeeded,
                "Tạo nguyên liệu thành công.",
                nguyenLieuRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<NguyenLieuRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi tạo nguyên liệu: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<NguyenLieuRespond>> UpdateNguyenLieu(string id, RequestUpdateNguyenLieu request)
    {
        try
        {
            var filter = Builders<NguyenLieu>.Filter.Eq(x => x.Id, id);
            filter &= Builders<NguyenLieu>.Filter.Eq(x => x.isDelete, false);
            var nguyenLieu = await _collection.Find(filter).FirstOrDefaultAsync();

            if (nguyenLieu == null)
            {
                return new RespondAPI<NguyenLieuRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy nguyên liệu với ID đã cung cấp."
                );
            }

            _mapper.Map(request, nguyenLieu);

            nguyenLieu.updatedDate = DateTimeOffset.UtcNow;

            // Cập nhật người dùng nếu có thông tin
            // danhMucNguyenLieu.updatedUser = currentUser.Id;

            var updateResult = await _collection.ReplaceOneAsync(filter, nguyenLieu);

            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                return new RespondAPI<NguyenLieuRespond>(
                    ResultRespond.Error,
                    "Cập nhật nguyên liệu không thành công."
                );
            }

            var loaiNguyenLieu = await _collectionLoaiNguyenLieu.Find(x => x.Id == nguyenLieu.loaiNguyenLieu).FirstOrDefaultAsync();
            var donViTinh = await _collectionDonViTinh.Find(x => x.Id == nguyenLieu.donViTinh).FirstOrDefaultAsync();
            var tuDo = await _collectionTuDo.Find(x => x.Id == nguyenLieu.tuDo).FirstOrDefaultAsync();

            var nguyenLieuRespond = new NguyenLieuRespond
            {
                id = nguyenLieu.Id,
                tenNguyenLieu = nguyenLieu.tenNguyenLieu,
                moTa = nguyenLieu.moTa,
                soLuong = nguyenLieu.soLuong,
                trangThai = nguyenLieu.trangThai,
                loaiNguyenLieu = new IdName
                {
                    Id = loaiNguyenLieu.Id,
                    Name = loaiNguyenLieu.tenLoai
                },
                donViTinh = new IdName
                {
                    Id = donViTinh.Id,
                    Name = donViTinh.tenDonViTinh
                },
                tuDo = new IdName
                {
                    Id = tuDo.Id,
                    Name = tuDo.tenTuDo
                }
            };
            return new RespondAPI<NguyenLieuRespond>(
                ResultRespond.Succeeded,
                "Cập nhật nguyên liệu thành công.",
                nguyenLieuRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<NguyenLieuRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi cập nhật nguyên liệu: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<string>> DeleteNguyenLieu(string id)
    {
        try
        {
            var existingNguyenLieu = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();
            if (existingNguyenLieu == null)
            {
                return new RespondAPI<string>(
                    ResultRespond.NotFound,
                    "Không tìm thấy nguyên liệu để xóa."
                );
            }

            var deleteResult = await _collection.DeleteOneAsync(x => x.Id == id);

            if (deleteResult.DeletedCount == 0)
            {
                return new RespondAPI<string>(
                    ResultRespond.Error,
                    "Xóa nguyên liệu không thành công."
                );
            }

            return new RespondAPI<string>(
                ResultRespond.Succeeded,
                "Xóa nguyên liệu thành công.",
                id
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<string>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi xóa nguyên liệu: {ex.Message}"
            );
        }
    }
}