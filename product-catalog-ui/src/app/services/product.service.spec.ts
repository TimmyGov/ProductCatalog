import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ProductService } from './product.service';
import { Product, CreateProduct, UpdateProduct } from '../models';
import { environment } from '../../environments/environment';

describe('ProductService', () => {
  let service: ProductService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiUrl}/api/products`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ProductService]
    });
    service = TestBed.inject(ProductService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should fetch products with pagination', () => {
    const mockResponse = {
      products: [
        {
          id: 1,
          name: 'Test Product',
          sku: 'TEST-001',
          description: 'Test Description',
          price: 99.99,
          quantity: 10,
          categoryId: 1,
          categoryName: 'Test Category',
          createdAt: new Date('2024-01-01'),
          updatedAt: new Date('2024-01-01')
        }
      ],
      totalCount: 1
    };

    service.getProducts(1, 10).subscribe(response => {
      expect(response.products.length).toBe(1);
      expect(response.totalCount).toBe(1);
      expect(response.products[0].name).toBe('Test Product');
    });

    const req = httpMock.expectOne(`${apiUrl}?page=1&pageSize=10`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should fetch a single product by id', () => {
    const mockProduct: Product = {
      id: 1,
      name: 'Test Product',
      sku: 'TEST-001',
      description: 'Test Description',
      price: 99.99,
      quantity: 10,
      categoryId: 1,
      categoryName: 'Test Category',
      createdAt: new Date('2024-01-01'),
      updatedAt: new Date('2024-01-01')
    };

    service.getProduct(1).subscribe(product => {
      expect(product.id).toBe(1);
      expect(product.name).toBe('Test Product');
    });

    const req = httpMock.expectOne(`${apiUrl}/1`);
    expect(req.request.method).toBe('GET');
    req.flush(mockProduct);
  });

  it('should create a new product', () => {
    const newProduct: CreateProduct = {
      name: 'New Product',
      sku: 'NEW-001',
      description: 'New Description',
      price: 49.99,
      quantity: 5,
      categoryId: 1
    };

    const mockResponse: Product = {
      id: 2,
      ...newProduct,
      categoryName: 'Test Category',
      createdAt: new Date('2024-01-02'),
      updatedAt: new Date('2024-01-02')
    };

    service.createProduct(newProduct).subscribe(product => {
      expect(product.id).toBe(2);
      expect(product.name).toBe('New Product');
    });

    const req = httpMock.expectOne(apiUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(newProduct);
    req.flush(mockResponse);
  });

  it('should update an existing product', () => {
    const updateData: UpdateProduct = {
      name: 'Updated Product',
      price: 79.99
    };

    const mockResponse: Product = {
      id: 1,
      name: 'Updated Product',
      sku: 'TEST-001',
      description: 'Test Description',
      price: 79.99,
      quantity: 10,
      categoryId: 1,
      categoryName: 'Test Category',
      createdAt: new Date('2024-01-01'),
      updatedAt: new Date('2024-01-02')
    };

    service.updateProduct(1, updateData).subscribe(product => {
      expect(product.name).toBe('Updated Product');
      expect(product.price).toBe(79.99);
    });

    const req = httpMock.expectOne(`${apiUrl}/1`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(updateData);
    req.flush(mockResponse);
  });

  it('should delete a product', () => {
    service.deleteProduct(1).subscribe();

    const req = httpMock.expectOne(`${apiUrl}/1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('should handle errors', () => {
    const errorMessage = 'Product not found';

    service.getProduct(999).subscribe({
      next: () => fail('should have failed with 404 error'),
      error: (error) => {
        expect(error.message).toBeDefined();
      }
    });

    const req = httpMock.expectOne(`${apiUrl}/999`);
    req.flush({ message: errorMessage }, { status: 404, statusText: 'Not Found' });
  });
});
