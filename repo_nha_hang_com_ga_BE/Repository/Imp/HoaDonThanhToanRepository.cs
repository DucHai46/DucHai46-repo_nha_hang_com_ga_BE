
using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models;
using repo_nha_hang_com_ga_BE.Models.Common.Models.Respond;
using repo_nha_hang_com_ga_BE.Models.Common.Paging;
using repo_nha_hang_com_ga_BE.Models.Common.Respond;
using repo_nha_hang_com_ga_BE.Models.MongoDB;
using repo_nha_hang_com_ga_BE.Models.Requests.HoaDonThanhToan;
using repo_nha_hang_com_ga_BE.Models.Responds.HoaDonThanhToan;


namespace repo_nha_hang_com_ga_BE.Repository.Imp;

public class HoaDonThanhToanRepository : IHoaDonThanhToanRepository
{
    private readonly IMongoCollection<HoaDonThanhToan> _collection;
    private readonly IMongoCollection<NhanVien> _collectionNhanVien;
    private readonly IMongoCollection<DonOrder> _collectionDonOrder;
    private readonly IMongoCollection<PhuongThucThanhToan> _collectionPhuongThucThanhToan;
    private readonly IMongoCollection<NhaHang> _collectionNhaHang;
    private readonly IMongoCollection<KhuyenMai> _collectionKhuyenMai;
    private readonly IMongoCollection<PhuPhi> _collectionPhuPhi;

    private readonly IMapper _mapper;

    public HoaDonThanhToanRepository(IOptions<MongoDbSettings> settings, IMapper mapper)
    {
        var mongoClientSettings = settings.Value;
        var client = new MongoClient(mongoClientSettings.Connection);
        var database = client.GetDatabase(mongoClientSettings.DatabaseName);
        _collection = database.GetCollection<HoaDonThanhToan>("HoaDonThanhToan");
        _collectionNhanVien = database.GetCollection<NhanVien>("NhanVien");
        _collectionDonOrder = database.GetCollection<DonOrder>("DonOrder");
        _collectionPhuongThucThanhToan = database.GetCollection<PhuongThucThanhToan>("PhuongThucThanhToan");
        _collectionNhaHang = database.GetCollection<NhaHang>("NhaHang");
        _collectionKhuyenMai = database.GetCollection<KhuyenMai>("KhuyenMai");
        _collectionPhuPhi = database.GetCollection<PhuPhi>("PhuPhi");
        _mapper = mapper;
    }

    public async Task<RespondAPIPaging<List<HoaDonThanhToanRespond>>> GetAllHoaDonThanhToan(RequestSearchHoaDonThanhToan request)
    {
        try
        {
            var collection = _collection;

            var filter = Builders<HoaDonThanhToan>.Filter.Empty;
            filter &= Builders<HoaDonThanhToan>.Filter.Eq(x => x.isDelete, false);

            if (!string.IsNullOrEmpty(request.nhanVien))
            {
                filter &= Builders<HoaDonThanhToan>.Filter.Eq(x => x.nhanVien, request.nhanVien);
            }

            if (!string.IsNullOrEmpty(request.donOrder))
            {
                filter &= Builders<HoaDonThanhToan>.Filter.Eq(x => x.donOrder, request.donOrder);
            }

            if (!string.IsNullOrEmpty(request.phuongThucThanhToan))
            {
                filter &= Builders<HoaDonThanhToan>.Filter.Eq(x => x.phuongThucThanhToan, request.phuongThucThanhToan);
            }

            // if (!string.IsNullOrEmpty(request.nhaHang))
            // {
            //     filter &= Builders<HoaDonThanhToan>.Filter.Eq(x => x.nhaHang, request.nhaHang);
            // }


            if (!string.IsNullOrEmpty(request.tenHoaDon))
            {
                filter &= Builders<HoaDonThanhToan>.Filter.Regex(x => x.tenHoaDon, request.tenHoaDon);
            }

            if (request.gioVao.HasValue)
            {
                filter &= Builders<HoaDonThanhToan>.Filter.Eq(x => x.gioVao, request.gioVao);
            }

            if (request.gioRa.HasValue)
            {
                filter &= Builders<HoaDonThanhToan>.Filter.Eq(x => x.gioRa, request.gioRa);
            }

            if (request.soNguoi.HasValue)
            {
                filter &= Builders<HoaDonThanhToan>.Filter.Eq(x => x.soNguoi, request.soNguoi);
            }

            if (request.trangthai.HasValue) // Kiểm tra nếu trangThai có giá trị: True hoặc False
            {
                filter &= Builders<HoaDonThanhToan>.Filter.Eq(x => x.trangthai, request.trangthai);
            }

            var projection = Builders<HoaDonThanhToan>.Projection
              .Include(x => x.Id)
              .Include(x => x.nhanVien)
              .Include(x => x.donOrder)
              .Include(x => x.phuongThucThanhToan)
              .Include(x => x.nhaHang)
             .Include(x => x.tenHoaDon)
             .Include(x => x.qrCode)
             .Include(x => x.gioVao)
             .Include(x => x.gioRa)
             .Include(x => x.soNguoi)
             .Include(x => x.khuyenMai)
             .Include(x => x.phuPhi)
            .Include(x => x.trangthai);

            var findOptions = new FindOptions<HoaDonThanhToan, HoaDonThanhToan>

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
                var hoadons = await cursor.ToListAsync();

                // Lấy danh sách các Id từ danh sách hóa đơn
                var nhanVienIds = hoadons.Select(x => x.nhanVien).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                var donOrderIds = hoadons.Select(x => x.donOrder).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                var nhaHangIds = hoadons.Select(x => x.nhaHang).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                var phuongThucThanhToanIds = hoadons.Select(x => x.phuongThucThanhToan).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                var khuyenMaiIds = hoadons.Select(x => x.khuyenMai).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                var phuPhiIds = hoadons.Select(x => x.phuPhi).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();


                // Query tới các bảng khác
                var nhanVienFilter = Builders<NhanVien>.Filter.In(x => x.Id, nhanVienIds);
                var nhanVienProjection = Builders<NhanVien>.Projection
                 .Include(x => x.Id)
                .Include(x => x.tenNhanVien);
                var nhanViens = await _collectionNhanVien.Find(nhanVienFilter)
                .Project<NhanVien>(nhanVienProjection).ToListAsync();

                var donOrderFilter = Builders<DonOrder>.Filter.In(x => x.Id, donOrderIds);
                var donOrderProjection = Builders<DonOrder>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenDon);
                var donOrders = await _collectionDonOrder.Find(donOrderFilter)
                .Project<DonOrder>(donOrderProjection).ToListAsync();

                // Thêm điều kiện isActive cho nhà hàng
                var nhaHangFilter = Builders<NhaHang>.Filter.In(x => x.Id, nhaHangIds);
                // nhaHangFilter &= Builders<NhaHang>.Filter.Eq(x => x.isActive, true); // Chỉ lấy nhà hàng đang hoạt động
                var nhaHangProjection = Builders<NhaHang>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenNhaHang)
                    .Include(x => x.isActive);
                var nhaHangs = await _collectionNhaHang.Find(nhaHangFilter)
                    .Project<NhaHang>(nhaHangProjection).ToListAsync();

                var phuongThucThanhToanFilter = Builders<PhuongThucThanhToan>.Filter.In(x => x.Id, phuongThucThanhToanIds);
                var phuongThucThanhToanProjection = Builders<PhuongThucThanhToan>.Projection
                    .Include(x => x.Id)
                   .Include(x => x.tenPhuongThuc);
                var phuongThucThanhToans = await _collectionPhuongThucThanhToan.Find(phuongThucThanhToanFilter)
                   .Project<PhuongThucThanhToan>(phuongThucThanhToanProjection).ToListAsync();

                var khuyenMaiFilter = Builders<KhuyenMai>.Filter.In(x => x.Id, khuyenMaiIds);
                var khuyenMaiProjection = Builders<KhuyenMai>.Projection
                   .Include(x => x.Id)
                  .Include(x => x.tenKhuyenMai);
                var khuyenMais = await _collectionKhuyenMai.Find(khuyenMaiFilter)
                  .Project<KhuyenMai>(khuyenMaiProjection).ToListAsync();

                var phuPhiFilter = Builders<PhuPhi>.Filter.In(x => x.Id, phuPhiIds);
                var phuPhiProjection = Builders<PhuPhi>.Projection
                  .Include(x => x.Id)
                 .Include(x => x.tenPhuPhi);
                var phuPhis = await _collectionPhuPhi.Find(phuPhiFilter)
                    .Project<PhuPhi>(phuPhiProjection).ToListAsync();


                // Tạo dictionary để map nhanh
                var nhanVienDict = nhanViens.ToDictionary(x => x.Id, x => x.tenNhanVien);
                var donOrderDict = donOrders.ToDictionary(x => x.Id, x => x.tenDon);
                var nhaHangDict = nhaHangs.ToDictionary(x => x.Id, x => x.tenNhaHang);
                var phuongThucThanhToanDict = phuongThucThanhToans.ToDictionary(x => x.Id, x => x.tenPhuongThuc);
                var khuyenMaiDict = khuyenMais.ToDictionary(x => x.Id, x => x.tenKhuyenMai);
                var phuPhiDict = phuPhis.ToDictionary(x => x.Id, x => x.tenPhuPhi);

                // Map dữ liệu
                var hoaDonThanhToanResponds = hoadons.Select(x => new HoaDonThanhToanRespond
                {
                    id = x.Id,
                    nhanVien = new IdName
                    {
                        Id = x.nhanVien,
                        Name = x.nhanVien != null && nhanVienDict.ContainsKey(x.nhanVien) ? nhanVienDict[x.nhanVien] : null
                    },
                    donOrder = new IdName
                    {
                        Id = x.donOrder,
                        Name = x.donOrder != null && donOrderDict.ContainsKey(x.donOrder) ? donOrderDict[x.donOrder] : null
                    },
                    phuongThucThanhToan = new IdName
                    {
                        Id = x.phuongThucThanhToan,
                        Name = x.phuongThucThanhToan != null && phuongThucThanhToanDict.ContainsKey(x.phuongThucThanhToan) ? phuongThucThanhToanDict[x.phuongThucThanhToan] : null
                    },
                    nhaHang = new IdName
                    {
                        Id = x.nhaHang,
                        Name = x.nhaHang != null && nhaHangDict.ContainsKey(x.nhaHang) ? nhaHangDict[x.nhaHang] : null
                    },
                    tenHoaDon = x.tenHoaDon,
                    qrCode = x.qrCode,
                    gioVao = x.gioVao,
                    gioRa = x.gioRa,
                    soNguoi = x.soNguoi,
                    khuyenMai = new IdName
                    {
                        Id = x.khuyenMai,
                        Name = x.khuyenMai != null && khuyenMaiDict.ContainsKey(x.khuyenMai) ? khuyenMaiDict[x.khuyenMai] : null
                    },
                    phuPhi = new IdName
                    {
                        Id = x.phuPhi,
                        Name = x.phuPhi != null && phuPhiDict.ContainsKey(x.phuPhi) ? phuPhiDict[x.phuPhi] : null
                    },
                    trangthai = x.trangthai
                }).ToList();

                var pagingDetail = new PagingDetail(currentPage, request.PageSize, totalRecords);
                var pagingResponse = new PagingResponse<List<HoaDonThanhToanRespond>>
                {
                    Paging = pagingDetail,
                    Data = hoaDonThanhToanResponds
                };

                return new RespondAPIPaging<List<HoaDonThanhToanRespond>>(
                    ResultRespond.Succeeded,
                    data: pagingResponse
                );
            }
            else
            {
                var cursor = await collection.FindAsync(filter, findOptions);
                var hoadons = await cursor.ToListAsync();

                // Lấy danh sách các Id từ danh sách hóa đơn
                var nhanVienIds = hoadons.Select(x => x.nhanVien).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                var donOrderIds = hoadons.Select(x => x.donOrder).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                var nhaHangIds = hoadons.Select(x => x.nhaHang).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                var phuongThucThanhToanIds = hoadons.Select(x => x.phuongThucThanhToan).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                var khuyenMaiIds = hoadons.Select(x => x.khuyenMai).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                var phuPhiIds = hoadons.Select(x => x.phuPhi).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();


                // Query tới các bảng khác
                var nhanVienFilter = Builders<NhanVien>.Filter.In(x => x.Id, nhanVienIds);
                var nhanVienProjection = Builders<NhanVien>.Projection
                 .Include(x => x.Id)
                .Include(x => x.tenNhanVien);
                var nhanViens = await _collectionNhanVien.Find(nhanVienFilter)
                .Project<NhanVien>(nhanVienProjection).ToListAsync();

                var donOrderFilter = Builders<DonOrder>.Filter.In(x => x.Id, donOrderIds);
                var donOrderProjection = Builders<DonOrder>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenDon);
                var donOrders = await _collectionDonOrder.Find(donOrderFilter)
                .Project<DonOrder>(donOrderProjection).ToListAsync();

                var nhaHangFilter = Builders<NhaHang>.Filter.In(x => x.Id, nhaHangIds);
                var nhaHangProjection = Builders<NhaHang>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenNhaHang);
                var nhaHangs = await _collectionNhaHang.Find(nhaHangFilter)
                    .Project<NhaHang>(nhaHangProjection).ToListAsync();

                var phuongThucThanhToanFilter = Builders<PhuongThucThanhToan>.Filter.In(x => x.Id, phuongThucThanhToanIds);
                var phuongThucThanhToanProjection = Builders<PhuongThucThanhToan>.Projection
                    .Include(x => x.Id)
                   .Include(x => x.tenPhuongThuc);
                var phuongThucThanhToans = await _collectionPhuongThucThanhToan.Find(phuongThucThanhToanFilter)
                   .Project<PhuongThucThanhToan>(phuongThucThanhToanProjection).ToListAsync();

                var khuyenMaiFilter = Builders<KhuyenMai>.Filter.In(x => x.Id, khuyenMaiIds);
                var khuyenMaiProjection = Builders<KhuyenMai>.Projection
                   .Include(x => x.Id)
                  .Include(x => x.tenKhuyenMai);
                var khuyenMais = await _collectionKhuyenMai.Find(khuyenMaiFilter)
                  .Project<KhuyenMai>(khuyenMaiProjection).ToListAsync();

                var phuPhiFilter = Builders<PhuPhi>.Filter.In(x => x.Id, phuPhiIds);
                var phuPhiProjection = Builders<PhuPhi>.Projection
                  .Include(x => x.Id)
                 .Include(x => x.tenPhuPhi);
                var phuPhis = await _collectionPhuPhi.Find(phuPhiFilter)
                    .Project<PhuPhi>(phuPhiProjection).ToListAsync();


                // Tạo dictionary để map nhanh
                var nhanVienDict = nhanViens.ToDictionary(x => x.Id, x => x.tenNhanVien);
                var donOrderDict = donOrders.ToDictionary(x => x.Id, x => x.tenDon);
                var nhaHangDict = nhaHangs.ToDictionary(x => x.Id, x => x.tenNhaHang);
                var phuongThucThanhToanDict = phuongThucThanhToans.ToDictionary(x => x.Id, x => x.tenPhuongThuc);
                var khuyenMaiDict = khuyenMais.ToDictionary(x => x.Id, x => x.tenKhuyenMai);
                var phuPhiDict = phuPhis.ToDictionary(x => x.Id, x => x.tenPhuPhi);

                // Map dữ liệu
                var hoaDonThanhToanResponds = hoadons.Select(x => new HoaDonThanhToanRespond
                {
                    id = x.Id,
                    nhanVien = new IdName
                    {
                        Id = x.nhanVien,
                        Name = x.nhanVien != null && nhanVienDict.ContainsKey(x.nhanVien) ? nhanVienDict[x.nhanVien] : null
                    },
                    donOrder = new IdName
                    {
                        Id = x.donOrder,
                        Name = x.donOrder != null && donOrderDict.ContainsKey(x.donOrder) ? donOrderDict[x.donOrder] : null
                    },
                    phuongThucThanhToan = new IdName
                    {
                        Id = x.phuongThucThanhToan,
                        Name = x.phuongThucThanhToan != null && phuongThucThanhToanDict.ContainsKey(x.phuongThucThanhToan) ? phuongThucThanhToanDict[x.phuongThucThanhToan] : null
                    },
                    nhaHang = new IdName
                    {
                        Id = x.nhaHang,
                        Name = x.nhaHang != null && nhaHangDict.ContainsKey(x.nhaHang) ? nhaHangDict[x.nhaHang] : null
                    },
                    tenHoaDon = x.tenHoaDon,
                    qrCode = x.qrCode,
                    gioVao = x.gioVao,
                    gioRa = x.gioRa,
                    soNguoi = x.soNguoi,
                    khuyenMai = new IdName
                    {
                        Id = x.khuyenMai,
                        Name = x.khuyenMai != null && khuyenMaiDict.ContainsKey(x.khuyenMai) ? khuyenMaiDict[x.khuyenMai] : null
                    },
                    phuPhi = new IdName
                    {
                        Id = x.phuPhi,
                        Name = x.phuPhi != null && phuPhiDict.ContainsKey(x.phuPhi) ? phuPhiDict[x.phuPhi] : null
                    },
                    trangthai = x.trangthai
                }).ToList();

                return new RespondAPIPaging<List<HoaDonThanhToanRespond>>(
                    ResultRespond.Succeeded,
                    data: new PagingResponse<List<HoaDonThanhToanRespond>>
                    {
                        Data = hoaDonThanhToanResponds,
                        Paging = new PagingDetail(1, hoaDonThanhToanResponds.Count(), hoaDonThanhToanResponds.Count())
                    }
                );
            }
        }
        catch (Exception ex)
        {
            return new RespondAPIPaging<List<HoaDonThanhToanRespond>>(
                ResultRespond.Error,
                message: ex.Message
            );
        }
    }

    public async Task<RespondAPI<HoaDonThanhToanRespond>> GetHoaDonThanhToanById(string id)
    {
        try
        {
            var hoaDonThanhToan = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();

            if (hoaDonThanhToan == null)
            {
                return new RespondAPI<HoaDonThanhToanRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy hóa đơn thanh toán với ID đã cung cấp."
                );
            }

            // Lấy danh sách các Id từ danh sách hóa đơn
            var nhanVienIds = new List<string> { hoaDonThanhToan.nhanVien }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var donOrderIds = new List<string> { hoaDonThanhToan.donOrder }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var nhaHangIds = new List<string> { hoaDonThanhToan.nhaHang }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var phuongThucThanhToanIds = new List<string> { hoaDonThanhToan.phuongThucThanhToan }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var khuyenMaiIds = new List<string> { hoaDonThanhToan.khuyenMai }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var phuPhiIds = new List<string> { hoaDonThanhToan.phuPhi }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();


            // Query tới các bảng khác
            var nhanVienFilter = Builders<NhanVien>.Filter.In(x => x.Id, nhanVienIds);
            var nhanVienProjection = Builders<NhanVien>.Projection
             .Include(x => x.Id)
            .Include(x => x.tenNhanVien);
            var nhanViens = await _collectionNhanVien.Find(nhanVienFilter)
            .Project<NhanVien>(nhanVienProjection).ToListAsync();

            var donOrderFilter = Builders<DonOrder>.Filter.In(x => x.Id, donOrderIds);
            var donOrderProjection = Builders<DonOrder>.Projection
            .Include(x => x.Id)
            .Include(x => x.tenDon);
            var donOrders = await _collectionDonOrder.Find(donOrderFilter)
            .Project<DonOrder>(donOrderProjection).ToListAsync();

            var nhaHangFilter = Builders<NhaHang>.Filter.In(x => x.Id, nhaHangIds);
            var nhaHangProjection = Builders<NhaHang>.Projection
                .Include(x => x.Id)
                .Include(x => x.tenNhaHang);
            var nhaHangs = await _collectionNhaHang.Find(nhaHangFilter)
                .Project<NhaHang>(nhaHangProjection).ToListAsync();

            var phuongThucThanhToanFilter = Builders<PhuongThucThanhToan>.Filter.In(x => x.Id, phuongThucThanhToanIds);
            var phuongThucThanhToanProjection = Builders<PhuongThucThanhToan>.Projection
                .Include(x => x.Id)
               .Include(x => x.tenPhuongThuc);
            var phuongThucThanhToans = await _collectionPhuongThucThanhToan.Find(phuongThucThanhToanFilter)
               .Project<PhuongThucThanhToan>(phuongThucThanhToanProjection).ToListAsync();

            var khuyenMaiFilter = Builders<KhuyenMai>.Filter.In(x => x.Id, khuyenMaiIds);
            var khuyenMaiProjection = Builders<KhuyenMai>.Projection
               .Include(x => x.Id)
              .Include(x => x.tenKhuyenMai);
            var khuyenMais = await _collectionKhuyenMai.Find(khuyenMaiFilter)
              .Project<KhuyenMai>(khuyenMaiProjection).ToListAsync();

            var phuPhiFilter = Builders<PhuPhi>.Filter.In(x => x.Id, phuPhiIds);
            var phuPhiProjection = Builders<PhuPhi>.Projection
              .Include(x => x.Id)
             .Include(x => x.tenPhuPhi);
            var phuPhis = await _collectionPhuPhi.Find(phuPhiFilter)
                .Project<PhuPhi>(phuPhiProjection).ToListAsync();


            // Tạo dictionary để map nhanh
            var nhanVienDict = nhanViens.ToDictionary(x => x.Id, x => x.tenNhanVien);
            var donOrderDict = donOrders.ToDictionary(x => x.Id, x => x.tenDon);
            var nhaHangDict = nhaHangs.ToDictionary(x => x.Id, x => x.tenNhaHang);
            var phuongThucThanhToanDict = phuongThucThanhToans.ToDictionary(x => x.Id, x => x.tenPhuongThuc);
            var khuyenMaiDict = khuyenMais.ToDictionary(x => x.Id, x => x.tenKhuyenMai);
            var phuPhiDict = phuPhis.ToDictionary(x => x.Id, x => x.tenPhuPhi);

            // Map dữ liệu
            var hoaDonThanhToanResponds = new HoaDonThanhToanRespond
            {
                id = hoaDonThanhToan.Id,
                nhanVien = new IdName
                {
                    Id = hoaDonThanhToan.nhanVien,
                    Name = hoaDonThanhToan.nhanVien != null && nhanVienDict.ContainsKey(hoaDonThanhToan.nhanVien) ? nhanVienDict[hoaDonThanhToan.nhanVien] : null
                },
                donOrder = new IdName
                {
                    Id = hoaDonThanhToan.donOrder,
                    Name = hoaDonThanhToan.donOrder != null && donOrderDict.ContainsKey(hoaDonThanhToan.donOrder) ? donOrderDict[hoaDonThanhToan.donOrder] : null
                },
                phuongThucThanhToan = new IdName
                {
                    Id = hoaDonThanhToan.phuongThucThanhToan,
                    Name = hoaDonThanhToan.phuongThucThanhToan != null && phuongThucThanhToanDict.ContainsKey(hoaDonThanhToan.phuongThucThanhToan) ? phuongThucThanhToanDict[hoaDonThanhToan.phuongThucThanhToan] : null
                },
                nhaHang = new IdName
                {
                    Id = hoaDonThanhToan.nhaHang,
                    Name = hoaDonThanhToan.nhaHang != null && nhaHangDict.ContainsKey(hoaDonThanhToan.nhaHang) ? nhaHangDict[hoaDonThanhToan.nhaHang] : null
                },
                tenHoaDon = hoaDonThanhToan.tenHoaDon,
                qrCode = hoaDonThanhToan.qrCode,
                gioVao = hoaDonThanhToan.gioVao,
                gioRa = hoaDonThanhToan.gioRa,
                soNguoi = hoaDonThanhToan.soNguoi,
                khuyenMai = new IdName
                {
                    Id = hoaDonThanhToan.khuyenMai,
                    Name = hoaDonThanhToan.khuyenMai != null && khuyenMaiDict.ContainsKey(hoaDonThanhToan.khuyenMai) ? khuyenMaiDict[hoaDonThanhToan.khuyenMai] : null
                },
                phuPhi = new IdName
                {
                    Id = hoaDonThanhToan.phuPhi,
                    Name = hoaDonThanhToan.phuPhi != null && phuPhiDict.ContainsKey(hoaDonThanhToan.phuPhi) ? phuPhiDict[hoaDonThanhToan.phuPhi] : null
                },
                trangthai = hoaDonThanhToan.trangthai,
            };

            return new RespondAPI<HoaDonThanhToanRespond>(
                ResultRespond.Succeeded,
                "Lấy hóa đơn thanh toán thành công.",
                hoaDonThanhToanResponds
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<HoaDonThanhToanRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<HoaDonThanhToanRespond>> CreateHoaDonThanhToan(RequestAddHoaDonThanhToan request)
    {
        try
        {
            // Kiểm tra xem nhà hàng có tồn tại và đang hoạt động không
            if (!string.IsNullOrEmpty(request.nhaHang))
            {
                var nhaHangActiveFilter = Builders<NhaHang>.Filter.Eq(x => x.Id, request.nhaHang);
                nhaHangActiveFilter &= Builders<NhaHang>.Filter.Eq(x => x.isActive, true);

                var nhaHangActive = await _collectionNhaHang.Find(nhaHangActiveFilter).AnyAsync();

                if (!nhaHangActive)
                {
                    return new RespondAPI<HoaDonThanhToanRespond>(
                        ResultRespond.Error,
                        "Không thể tạo hóa đơn vì nhà hàng không hoạt động."
                    );
                }
                else
                {
                    HoaDonThanhToan newHoaDonThanhToan = _mapper.Map<HoaDonThanhToan>(request);

                    newHoaDonThanhToan.createdDate = DateTimeOffset.UtcNow;
                    newHoaDonThanhToan.updatedDate = DateTimeOffset.UtcNow;
                    newHoaDonThanhToan.isDelete = false;
                    // Thiết lập createdUser và updatedUser nếu có thông tin người dùng
                    // newDanhMucMonAn.createdUser = currentUser.Id;
                    // newDanhMucNguyenLieu.updatedUser = currentUser.Id;

                    await _collection.InsertOneAsync(newHoaDonThanhToan);

                    // Tạo dictionary để map nhanh
                    var nhanVienDict = new Dictionary<string, string>();
                    var donOrderDict = new Dictionary<string, string>();
                    var nhaHangDict = new Dictionary<string, string>();
                    var phuongThucThanhToanDict = new Dictionary<string, string>();
                    var khuyenMaiDict = new Dictionary<string, string>();
                    var phuPhiDict = new Dictionary<string, string>();

                    // Lấy danh sách các Id từ danh sách hóa đơn
                    var nhanVienIds = new List<string> { newHoaDonThanhToan.nhanVien }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                    var donOrderIds = new List<string> { newHoaDonThanhToan.donOrder }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                    var nhaHangIds = new List<string> { newHoaDonThanhToan.nhaHang }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                    var phuongThucThanhToanIds = new List<string> { newHoaDonThanhToan.phuongThucThanhToan }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                    var khuyenMaiIds = new List<string> { newHoaDonThanhToan.khuyenMai }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                    var phuPhiIds = new List<string> { newHoaDonThanhToan.phuPhi }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();


                    // Query tới các bảng khác
                    var nhanVienFilter = Builders<NhanVien>.Filter.In(x => x.Id, nhanVienIds);
                    var nhanVienProjection = Builders<NhanVien>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.tenNhanVien);
                    var nhanViens = await _collectionNhanVien.Find(nhanVienFilter)
                        .Project<NhanVien>(nhanVienProjection).ToListAsync();
                    nhanVienDict = nhanViens.ToDictionary(x => x.Id, x => x.tenNhanVien);

                    var donOrderFilter = Builders<DonOrder>.Filter.In(x => x.Id, donOrderIds);
                    var donOrderProjection = Builders<DonOrder>.Projection
                        .Include(x => x.Id)
                       .Include(x => x.tenDon);
                    var donOrders = await _collectionDonOrder.Find(donOrderFilter)
                       .Project<DonOrder>(donOrderProjection).ToListAsync();
                    donOrderDict = donOrders.ToDictionary(x => x.Id, x => x.tenDon);

                    var nhaHangFilter = Builders<NhaHang>.Filter.In(x => x.Id, nhaHangIds);
                    var nhaHangProjection = Builders<NhaHang>.Projection
                       .Include(x => x.Id)
                       .Include(x => x.tenNhaHang);
                    var nhaHangs = await _collectionNhaHang.Find(nhaHangFilter)
                       .Project<NhaHang>(nhaHangProjection).ToListAsync();
                    nhaHangDict = nhaHangs.ToDictionary(x => x.Id, x => x.tenNhaHang);

                    var phuongThucThanhToanFilter = Builders<PhuongThucThanhToan>.Filter.In(x => x.Id, phuongThucThanhToanIds);
                    var phuongThucThanhToanProjection = Builders<PhuongThucThanhToan>.Projection
                        .Include(x => x.Id)
                    .Include(x => x.tenPhuongThuc);
                    var phuongThucThanhToans = await _collectionPhuongThucThanhToan.Find(phuongThucThanhToanFilter)
                      .Project<PhuongThucThanhToan>(phuongThucThanhToanProjection).ToListAsync();
                    phuongThucThanhToanDict = phuongThucThanhToans.ToDictionary(x => x.Id, x => x.tenPhuongThuc);

                    var khuyenMaiFilter = Builders<KhuyenMai>.Filter.In(x => x.Id, khuyenMaiIds);
                    var khuyenMaiProjection = Builders<KhuyenMai>.Projection
                      .Include(x => x.Id)
                     .Include(x => x.tenKhuyenMai);
                    var khuyenMais = await _collectionKhuyenMai.Find(khuyenMaiFilter)
                     .Project<KhuyenMai>(khuyenMaiProjection).ToListAsync();
                    khuyenMaiDict = khuyenMais.ToDictionary(x => x.Id, x => x.tenKhuyenMai);

                    var phuPhiFilter = Builders<PhuPhi>.Filter.In(x => x.Id, phuPhiIds);
                    var phuPhiProjection = Builders<PhuPhi>.Projection
                     .Include(x => x.Id)
                    .Include(x => x.tenPhuPhi);
                    var phuPhis = await _collectionPhuPhi.Find(phuPhiFilter)
                      .Project<PhuPhi>(phuPhiProjection).ToListAsync();
                    phuPhiDict = phuPhis.ToDictionary(x => x.Id, x => x.tenPhuPhi);

                    // Map dữ liệu
                    var hoaDonThanhToanResponds = new HoaDonThanhToanRespond
                    {
                        id = newHoaDonThanhToan.Id,
                        nhanVien = new IdName
                        {
                            Id = newHoaDonThanhToan.nhanVien,
                            Name = nhanVienDict.ContainsKey(newHoaDonThanhToan.nhanVien) ? nhanVienDict[newHoaDonThanhToan.nhanVien] : null
                        },
                        donOrder = new IdName
                        {
                            Id = newHoaDonThanhToan.donOrder,
                            Name = donOrderDict.ContainsKey(newHoaDonThanhToan.donOrder) ? donOrderDict[newHoaDonThanhToan.donOrder] : null
                        },
                        phuongThucThanhToan = new IdName
                        {
                            Id = newHoaDonThanhToan.phuongThucThanhToan,
                            Name = phuongThucThanhToanDict.ContainsKey(newHoaDonThanhToan.phuongThucThanhToan) ? phuongThucThanhToanDict[newHoaDonThanhToan.phuongThucThanhToan] : null
                        },
                        nhaHang = new IdName
                        {
                            Id = newHoaDonThanhToan.nhaHang,
                            Name = nhaHangDict.ContainsKey(newHoaDonThanhToan.nhaHang) ? nhaHangDict[newHoaDonThanhToan.nhaHang] : null
                        },
                        tenHoaDon = newHoaDonThanhToan.tenHoaDon,
                        qrCode = newHoaDonThanhToan.qrCode,
                        gioVao = newHoaDonThanhToan.gioVao,
                        gioRa = newHoaDonThanhToan.gioRa,
                        soNguoi = newHoaDonThanhToan.soNguoi,
                        khuyenMai = new IdName
                        {
                            Id = newHoaDonThanhToan.khuyenMai,
                            Name = khuyenMaiDict.ContainsKey(newHoaDonThanhToan.khuyenMai) ? khuyenMaiDict[newHoaDonThanhToan.khuyenMai] : null
                        },
                        phuPhi = new IdName
                        {
                            Id = newHoaDonThanhToan.phuPhi,
                            Name = phuPhiDict.ContainsKey(newHoaDonThanhToan.phuPhi) ? phuPhiDict[newHoaDonThanhToan.phuPhi] : null
                        },
                        trangthai = newHoaDonThanhToan.trangthai,
                    };

                    return new RespondAPI<HoaDonThanhToanRespond>(
                        ResultRespond.Succeeded,
                        "Tạo hóa đơn thanh toán thành công.",
                        hoaDonThanhToanResponds
                    );
                }
            }
            else
            {
                return new RespondAPI<HoaDonThanhToanRespond>(
                        ResultRespond.Error,
                        "Không thể tạo hóa đơn vì nhà hàng không tồn tại."
                    );
            }
        }
        catch (Exception ex)
        {
            return new RespondAPI<HoaDonThanhToanRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi tạo hóa đơn thanh toán: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<HoaDonThanhToanRespond>> UpdateHoaDonThanhToan(string id, RequestUpdateHoaDonThanhToan request)
    {
        try
        {
            var filter = Builders<HoaDonThanhToan>.Filter.Eq(x => x.Id, id);
            filter &= Builders<HoaDonThanhToan>.Filter.Eq(x => x.isDelete, false);
            var hoaDonThanhToan = await _collection.Find(filter).FirstOrDefaultAsync();

            if (hoaDonThanhToan == null)
            {
                return new RespondAPI<HoaDonThanhToanRespond>(
                    ResultRespond.NotFound,
                    "Không tìm thấy hóa đơn thanh toán với ID đã cung cấp."
                );
            }

            _mapper.Map(request, hoaDonThanhToan);

            hoaDonThanhToan.updatedDate = DateTimeOffset.UtcNow;

            // Cập nhật người dùng nếu có thông tin
            // danhMucNguyenLieu.updatedUser = currentUser.Id;

            var updateResult = await _collection.ReplaceOneAsync(filter, hoaDonThanhToan);

            if (!updateResult.IsAcknowledged || updateResult.ModifiedCount == 0)
            {
                return new RespondAPI<HoaDonThanhToanRespond>(
                    ResultRespond.Error,
                    "Cập nhật hóa đơn thanh toán không thành công."
                );
            }

            // Tạo dictionary để map nhanh
            var nhanVienDict = new Dictionary<string, string>();
            var donOrderDict = new Dictionary<string, string>();
            var nhaHangDict = new Dictionary<string, string>();
            var phuongThucThanhToanDict = new Dictionary<string, string>();
            var khuyenMaiDict = new Dictionary<string, string>();
            var phuPhiDict = new Dictionary<string, string>();

            // Lấy danh sách các Id từ danh sách hóa đơn
            var nhanVienIds = new List<string> { hoaDonThanhToan.nhanVien }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var donOrderIds = new List<string> { hoaDonThanhToan.donOrder }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var nhaHangIds = new List<string> { hoaDonThanhToan.nhaHang }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var phuongThucThanhToanIds = new List<string> { hoaDonThanhToan.phuongThucThanhToan }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var khuyenMaiIds = new List<string> { hoaDonThanhToan.khuyenMai }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var phuPhiIds = new List<string> { hoaDonThanhToan.phuPhi }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();


            // Query tới các bảng khác
            var nhanVienFilter = Builders<NhanVien>.Filter.In(x => x.Id, nhanVienIds);
            var nhanVienProjection = Builders<NhanVien>.Projection
            .Include(x => x.Id)
            .Include(x => x.tenNhanVien);
            var nhanViens = await _collectionNhanVien.Find(nhanVienFilter)
                .Project<NhanVien>(nhanVienProjection).ToListAsync();
            nhanVienDict = nhanViens.ToDictionary(x => x.Id, x => x.tenNhanVien);

            var donOrderFilter = Builders<DonOrder>.Filter.In(x => x.Id, donOrderIds);
            var donOrderProjection = Builders<DonOrder>.Projection
                .Include(x => x.Id)
               .Include(x => x.tenDon);
            var donOrders = await _collectionDonOrder.Find(donOrderFilter)
               .Project<DonOrder>(donOrderProjection).ToListAsync();
            donOrderDict = donOrders.ToDictionary(x => x.Id, x => x.tenDon);

            var nhaHangFilter = Builders<NhaHang>.Filter.In(x => x.Id, nhaHangIds);
            var nhaHangProjection = Builders<NhaHang>.Projection
               .Include(x => x.Id)
               .Include(x => x.tenNhaHang);
            var nhaHangs = await _collectionNhaHang.Find(nhaHangFilter)
               .Project<NhaHang>(nhaHangProjection).ToListAsync();
            nhaHangDict = nhaHangs.ToDictionary(x => x.Id, x => x.tenNhaHang);

            var phuongThucThanhToanFilter = Builders<PhuongThucThanhToan>.Filter.In(x => x.Id, phuongThucThanhToanIds);
            var phuongThucThanhToanProjection = Builders<PhuongThucThanhToan>.Projection
              .Include(x => x.Id)
            .Include(x => x.tenPhuongThuc);
            var phuongThucThanhToans = await _collectionPhuongThucThanhToan.Find(phuongThucThanhToanFilter)
              .Project<PhuongThucThanhToan>(phuongThucThanhToanProjection).ToListAsync();
            phuongThucThanhToanDict = phuongThucThanhToans.ToDictionary(x => x.Id, x => x.tenPhuongThuc);

            var khuyenMaiFilter = Builders<KhuyenMai>.Filter.In(x => x.Id, khuyenMaiIds);
            var khuyenMaiProjection = Builders<KhuyenMai>.Projection
              .Include(x => x.Id)
             .Include(x => x.tenKhuyenMai);
            var khuyenMais = await _collectionKhuyenMai.Find(khuyenMaiFilter)
             .Project<KhuyenMai>(khuyenMaiProjection).ToListAsync();
            khuyenMaiDict = khuyenMais.ToDictionary(x => x.Id, x => x.tenKhuyenMai);

            var phuPhiFilter = Builders<PhuPhi>.Filter.In(x => x.Id, phuPhiIds);
            var phuPhiProjection = Builders<PhuPhi>.Projection
             .Include(x => x.Id)
            .Include(x => x.tenPhuPhi);
            var phuPhis = await _collectionPhuPhi.Find(phuPhiFilter)
              .Project<PhuPhi>(phuPhiProjection).ToListAsync();
            phuPhiDict = phuPhis.ToDictionary(x => x.Id, x => x.tenPhuPhi);

            // Map dữ liệu
            var hoaDonThanhToanResponds = new HoaDonThanhToanRespond
            {
                id = hoaDonThanhToan.Id,
                nhanVien = new IdName
                {
                    Id = hoaDonThanhToan.nhanVien,
                    Name = nhanVienDict.ContainsKey(hoaDonThanhToan.nhanVien) ? nhanVienDict[hoaDonThanhToan.nhanVien] : null
                },
                donOrder = new IdName
                {
                    Id = hoaDonThanhToan.donOrder,
                    Name = donOrderDict.ContainsKey(hoaDonThanhToan.donOrder) ? donOrderDict[hoaDonThanhToan.donOrder] : null
                },
                phuongThucThanhToan = new IdName
                {
                    Id = hoaDonThanhToan.phuongThucThanhToan,
                    Name = phuongThucThanhToanDict.ContainsKey(hoaDonThanhToan.phuongThucThanhToan) ? phuongThucThanhToanDict[hoaDonThanhToan.phuongThucThanhToan] : null
                },
                nhaHang = new IdName
                {
                    Id = hoaDonThanhToan.nhaHang,
                    Name = nhaHangDict.ContainsKey(hoaDonThanhToan.nhaHang) ? nhaHangDict[hoaDonThanhToan.nhaHang] : null
                },
                tenHoaDon = hoaDonThanhToan.tenHoaDon,
                qrCode = hoaDonThanhToan.qrCode,
                gioVao = hoaDonThanhToan.gioVao,
                gioRa = hoaDonThanhToan.gioRa,
                soNguoi = hoaDonThanhToan.soNguoi,
                khuyenMai = new IdName
                {
                    Id = hoaDonThanhToan.khuyenMai,
                    Name = khuyenMaiDict.ContainsKey(hoaDonThanhToan.khuyenMai) ? khuyenMaiDict[hoaDonThanhToan.khuyenMai] : null
                },
                phuPhi = new IdName
                {
                    Id = hoaDonThanhToan.phuPhi,
                    Name = phuPhiDict.ContainsKey(hoaDonThanhToan.phuPhi) ? phuPhiDict[hoaDonThanhToan.phuPhi] : null
                },
                trangthai = hoaDonThanhToan.trangthai,
            };
            return new RespondAPI<HoaDonThanhToanRespond>(
                ResultRespond.Succeeded,
                "Cập nhật hóa đơn thanh toán thành công.",
                hoaDonThanhToanResponds
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<HoaDonThanhToanRespond>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi cập nhật hóa đơn thanh toán: {ex.Message}"
            );
        }
    }

    public async Task<RespondAPI<string>> DeleteHoaDonThanhToan(string id)
    {
        try
        {
            var existingHoaDonThanhToan = await _collection.Find(x => x.Id == id && x.isDelete == false).FirstOrDefaultAsync();
            if (existingHoaDonThanhToan == null)
            {
                return new RespondAPI<string>(
                    ResultRespond.NotFound,
                    "Không tìm thấy hóa đơn thanh toán để xóa."
                );
            }

            var deleteResult = await _collection.DeleteOneAsync(x => x.Id == id);

            if (deleteResult.DeletedCount == 0)
            {
                return new RespondAPI<string>(
                    ResultRespond.Error,
                    "Xóa hóa đơn thanh toán không thành công."
                );
            }

            return new RespondAPI<string>(
                ResultRespond.Succeeded,
                "Xóa hóa đơn thanh toán thành công.",
                id
            );
        }
        catch (Exception ex)
        {
            return new RespondAPI<string>(
                ResultRespond.Error,
                $"Đã xảy ra lỗi khi xóa hóa đơn thanh toán: {ex.Message}"
            );
        }
    }
}