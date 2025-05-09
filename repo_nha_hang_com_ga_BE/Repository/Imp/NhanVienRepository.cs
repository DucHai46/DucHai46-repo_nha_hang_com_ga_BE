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
using repo_nha_hang_com_ga_BE.Models.Requests.NhanVien;
using repo_nha_hang_com_ga_BE.Models.Responds.NhanVien;

namespace repo_nha_hang_com_ga_BE.Repository.Imp;

public class NhanVienRepository : INhanVienRepository
{
    private readonly IMongoCollection<NhanVien> _collection;
    private readonly IMongoCollection<ChucVu> _collectionChucVu;
    private readonly IMapper _mapper;

    public NhanVienRepository(IOptions<MongoDbSettings> settings, IMapper mapper)
    {
        var mongoClientSettings = settings.Value;
        var client = new MongoClient(mongoClientSettings.Connection);
        var database = client.GetDatabase(mongoClientSettings.DatabaseName);
        _collection = database.GetCollection<NhanVien>("NhanVien");
        _collectionChucVu = database.GetCollection<ChucVu>("ChucVu");
        _mapper = mapper;
    }

    public async Task<RespondAPIPaging<List<NhanVienRespond>>> GetAllNhanViens(RequestSearchNhanVien request)
    {
        try
        {
            var collection = _collection;

            var filter = Builders<NhanVien>.Filter.Empty;
            filter &= Builders<NhanVien>.Filter.Eq(x => x.isDelete, false);

            if (!string.IsNullOrEmpty(request.tenNhanVien))
            {
                filter &= Builders<NhanVien>.Filter.Regex(x => x.tenNhanVien, new BsonRegularExpression($".*{request.tenNhanVien}.*"));
            }

            if (!string.IsNullOrEmpty(request.chucVuId))
            {
                filter &= Builders<NhanVien>.Filter.Eq(x => x.chucVu, request.chucVuId);
            }

            var projection = Builders<NhanVien>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenNhanVien)
                .Include(x => x.chucVu)
                .Include(x => x.soDienThoai)
                .Include(x => x.email)
                .Include(x => x.diaChi)
                .Include(x => x.ngaySinh);

            var findOptions = new FindOptions<NhanVien, NhanVien>
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
                var nhanViens = await cursor.ToListAsync();

                var chucVuIds = nhanViens.Select(x => x.chucVu).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                var chucVuFilter = Builders<ChucVu>.Filter.In(x => x.Id, chucVuIds);
                var chucVuProjection = Builders<ChucVu>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenChucVu);
                var chucVus = await _collectionChucVu.Find(chucVuFilter)
                    .Project<ChucVu>(chucVuProjection)
                    .ToListAsync();

                // Tạo dictionary để map nhanh
                var chucVuDict = chucVus.ToDictionary(x => x.Id, x => x.tenChucVu);

                // Map dữ liệu
                var nhanVienResponds = nhanViens.Select(nhanVien => new NhanVienRespond
                {
                    id = nhanVien.Id,
                    tenNhanVien = nhanVien.tenNhanVien,
                    soDienThoai = nhanVien.soDienThoai,
                    email = nhanVien.email,
                    diaChi = nhanVien.diaChi,
                    ngaySinh = nhanVien.ngaySinh,
                    chucVu = new IdName
                    {
                        Id = nhanVien.chucVu,
                        Name = nhanVien.chucVu != null && chucVuDict.ContainsKey(nhanVien.chucVu) ? chucVuDict[nhanVien.chucVu] : null
                    }
                }).ToList();

                var pagingDetail = new PagingDetail(currentPage, request.PageSize, totalRecords);
                var pagingResponse = new PagingResponse<List<NhanVienRespond>>
                {
                    Paging = pagingDetail,
                    Data = nhanVienResponds
                };

                return new RespondAPIPaging<List<NhanVienRespond>>(
                    ResultRespond.Succeeded,
                    data: pagingResponse
                );
            }
            else
            {
                var cursor = await collection.FindAsync(filter, findOptions);
                var nhanViens = await cursor.ToListAsync();

                var chucVuIds = nhanViens.Select(x => x.chucVu).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                var chucVuFilter = Builders<ChucVu>.Filter.In(x => x.Id, chucVuIds);
                var chucVuProjection = Builders<ChucVu>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenChucVu);
                var chucVus = await _collectionChucVu.Find(chucVuFilter)
                    .Project<ChucVu>(chucVuProjection)
                    .ToListAsync();

                var chucVuDict = chucVus.ToDictionary(x => x.Id, x => x.tenChucVu);

                var nhanVienResponds = nhanViens.Select(nhanVien => new NhanVienRespond
                {
                    id = nhanVien.Id,
                    tenNhanVien = nhanVien.tenNhanVien,
                    soDienThoai = nhanVien.soDienThoai,
                    email = nhanVien.email,
                    diaChi = nhanVien.diaChi,
                    ngaySinh = nhanVien.ngaySinh,
                    chucVu = new IdName
                    {
                        Id = nhanVien.chucVu,
                        Name = nhanVien.chucVu != null && chucVuDict.ContainsKey(nhanVien.chucVu) ? chucVuDict[nhanVien.chucVu] : null
                    }
                }).ToList();

                return new RespondAPIPaging<List<NhanVienRespond>>(
                    ResultRespond.Succeeded,
                    data: new PagingResponse<List<NhanVienRespond>>
                    {
                        Data = nhanVienResponds,
                        Paging = new PagingDetail(1, nhanVienResponds.Count, nhanVienResponds.Count)
                    }
                );
            }
        }
        catch (Exception ex)
        {
            return new RespondAPIPaging<List<NhanVienRespond>>(
                ResultRespond.Error,
                message: ex.Message
            );
        }
    }

    public async Task<RespondAPI<NhanVienRespond>> GetNhanVienById(string id)
    {
        try
        {
            var nhanVien = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();


            if (nhanVien == null)
            {
                return new RespondAPI<NhanVienRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy nhân viên với ID đã cung cấp."
                );
            }

            // var nhanVienRespond = _mapper.Map<NhanVienRespond>(nhanVien);
            var chucVu = await _collectionChucVu.Find(x => x.Id == nhanVien.chucVu).FirstOrDefaultAsync();
            var nhanVienRespond = new NhanVienRespond
            {
                id = nhanVien.Id,
                tenNhanVien = nhanVien.tenNhanVien,
                soDienThoai = nhanVien.soDienThoai,
                email = nhanVien.email,
                diaChi = nhanVien.diaChi,
                ngaySinh = nhanVien.ngaySinh,
                chucVu = new IdName
                {
                    Id = chucVu.Id,
                    Name = chucVu.tenChucVu
                }
            };

            return new RespondAPI<NhanVienRespond>(
                ResultRespond.Succeeded,
                "Lấy nhân viên thành công.",
                nhanVienRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<NhanVienRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<NhanVienRespond>> CreateNhanVien(RequestAddNhanVien request)
    {
        try
        {
            NhanVien newNhanVien = _mapper.Map<NhanVien>(request);

            newNhanVien.createdDate = DateTimeOffset.UtcNow;
            newNhanVien.updatedDate = DateTimeOffset.UtcNow;
            newNhanVien.isDelete = false;
            // Thiết lập createdUser và updatedUser nếu có thông tin người dùng
            // newLoaiNguyenLieu.createdUser = currentUser.Id;
            // newDanhMucNguyenLieu.updatedUser = currentUser.Id;

            await _collection.InsertOneAsync(newNhanVien);

            // var nhanVienRespond = _mapper.Map<NhanVienRespond>(newNhanVien);
            var chucVu = await _collectionChucVu.Find(x => x.Id == newNhanVien.chucVu).FirstOrDefaultAsync();
            var nhanVienRespond = new NhanVienRespond
            {
                id = newNhanVien.Id,
                tenNhanVien = newNhanVien.tenNhanVien,
                soDienThoai = newNhanVien.soDienThoai,
                email = newNhanVien.email,
                diaChi = newNhanVien.diaChi,
                ngaySinh = newNhanVien.ngaySinh,
                chucVu = new IdName
                {
                    Id = chucVu.Id,
                    Name = chucVu.tenChucVu
                }
            };

            return new RespondAPI<NhanVienRespond>(
                ResultRespond.Succeeded,
                "Tạo nhân viên thành công.",
                nhanVienRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<NhanVienRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi tạo nhân viên: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<NhanVienRespond>> UpdateNhanVien(string id, RequestUpdateNhanVien request)
    {
        try
        {
            var filter = Builders<NhanVien>.Filter.Eq(x => x.Id, id);
            filter &= Builders<NhanVien>.Filter.Eq(x => x.isDelete, false);
            var nhanVien = await _collection.Find(filter).FirstOrDefaultAsync();

            if (nhanVien == null)
            {
                return new RespondAPI<NhanVienRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy nhân viên với ID đã cung cấp."
                );
            }

            _mapper.Map(request, nhanVien);

            nhanVien.updatedDate = DateTimeOffset.UtcNow;

            // Cập nhật người dùng nếu có thông tin
            // danhMucNguyenLieu.updatedUser = currentUser.Id;

            var updateResult = await _collection.ReplaceOneAsync(filter, nhanVien);

            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                return new RespondAPI<NhanVienRespond>(
                    ResultRespond.Error,
                    "Cập nhật nhân viên không thành công."
                );
            }

            // var nhanVienRespond = _mapper.Map<NhanVienRespond>(nhanVien);
            var chucVu = await _collectionChucVu.Find(x => x.Id == nhanVien.chucVu).FirstOrDefaultAsync();
            var nhanVienRespond = new NhanVienRespond
            {
                id = nhanVien.Id,
                tenNhanVien = nhanVien.tenNhanVien,
                soDienThoai = nhanVien.soDienThoai,
                email = nhanVien.email,
                diaChi = nhanVien.diaChi,
                ngaySinh = nhanVien.ngaySinh,
                chucVu = new IdName
                {
                    Id = chucVu.Id,
                    Name = chucVu.tenChucVu
                }
            };

            return new RespondAPI<NhanVienRespond>(
                ResultRespond.Succeeded,
                "Cập nhật nhân viên thành công.",
                nhanVienRespond
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<NhanVienRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi cập nhật nhân viên: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<string>> DeleteNhanVien(string id)
    {
        try
        {
            var existingNhanVien = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();
            if (existingNhanVien == null)
            {
                return new RespondAPI<string>(
                    ResultRespond.NotFound,
                    "Không tìm thấy nhân viên để xóa."
                );
            }

            var deleteResult = await _collection.DeleteOneAsync(x => x.Id == id);

            if (deleteResult.DeletedCount == 0)
            {
                return new RespondAPI<string>(
                    ResultRespond.Error,
                    "Xóa nhân viên không thành công."
                );
            }

            return new RespondAPI<string>(
                ResultRespond.Succeeded,
                "Xóa nhân viên thành công.",
                id
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<string>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi xóa nhân viên: {ex.Message}"
            );
        }
    }
}