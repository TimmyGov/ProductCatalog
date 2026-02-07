# Product Catalog Management System - Angular Frontend

This is the Angular frontend for the Product Catalog Management System. It provides a complete user interface for managing products and categories.

## Features

- **Product Management**: Create, read, update, and delete products
- **Search & Filter**: Real-time search with debounce and category filtering
- **Pagination**: Configurable page sizes (5, 10, 25, 50 items per page)
- **Sorting**: Sort products by name or price (ascending/descending)
- **Responsive Design**: Mobile-friendly interface with CSS media queries
- **Form Validation**: Comprehensive validation for all form inputs
- **TypeScript Strict Mode**: Full type safety throughout the application

## Architecture

### Models (`src/app/models/`)
- `Product` - Complete product interface with all fields
- `CreateProduct` - Interface for creating new products
- `UpdateProduct` - Interface for updating existing products
- `Category` - Category interface
- `CategoryTree` - Hierarchical category structure

### Services (`src/app/services/`)
- **ProductService**: Handles all product CRUD operations via HTTP
- **CategoryService**: Manages category data retrieval
- **SearchService**: Manages search state with RxJS debouncing (300ms)

### Components (`src/app/components/`)
- **ProductListComponent**: Displays products in a table with search, filter, sort, and pagination
- **ProductFormComponent**: Reactive form for creating/editing products with validation
- **SearchBarComponent**: Debounced search input
- **CategoryFilterComponent**: Category dropdown filter
- **LoadingSpinnerComponent**: Animated loading indicator
- **ErrorMessageComponent**: Error display component

### Routing
- `/` or `/products` - Product list view
- `/products/new` - Create new product
- `/products/:id/edit` - Edit existing product

## Technology Stack

- **Angular 16** - Standalone components
- **RxJS 7.8** - Reactive programming
- **TypeScript 5.1** - Strict mode enabled
- **Jasmine & Karma** - Unit testing

## Getting Started

### Prerequisites
- Node.js 18+ and npm

### Installation
```bash
npm install
```

### Development Server
```bash
npm start
```
Navigate to `http://localhost:4200/`. The app will automatically reload if you change any source files.

### Build
```bash
npm run build
```
Build artifacts will be stored in the `dist/` directory.

### Running Tests
```bash
npm test
```
Runs unit tests via Karma.

```bash
npm test -- --watch=false --browsers=ChromeHeadless
```
Run tests once in headless mode (CI/CD friendly).

## Configuration

### API Base URL
The API base URL is configured in environment files:
- Development: `src/environments/environment.ts`
- Production: `src/environments/environment.prod.ts`

Default: `http://localhost:5555`

## Code Structure

```
src/
├── app/
│   ├── components/       # Standalone components
│   │   ├── product-list/
│   │   ├── product-form/
│   │   ├── search-bar/
│   │   ├── category-filter/
│   │   ├── loading-spinner/
│   │   └── error-message/
│   ├── models/           # TypeScript interfaces
│   ├── services/         # HTTP and state management services
│   ├── app.component.*   # Root component
│   ├── app.routes.ts     # Route configuration
│   └── app.config.ts     # App configuration
├── environments/         # Environment configurations
└── styles.css           # Global styles
```

## Best Practices Implemented

1. **Standalone Components**: Modern Angular architecture
2. **RxJS Best Practices**:
   - Use of `async` pipe where possible
   - Proper subscription management with `takeUntil`
   - RxJS operators: `map`, `catchError`, `debounceTime`, `switchMap`
3. **Reactive Forms**: FormBuilder with comprehensive validation
4. **Error Handling**: Centralized error handling in services
5. **Type Safety**: TypeScript strict mode, no `any` types
6. **Responsive Design**: Mobile-first CSS with media queries
7. **Testing**: Unit tests for services with HttpClientTestingModule
8. **Clean Code**: Separation of concerns, reusable components

## Validation Rules

- **Name**: Required, minimum 3 characters
- **SKU**: Required
- **Price**: Required, must be positive (≥ 0)
- **Quantity**: Required, must be non-negative (≥ 0)
- **Category**: Optional
- **Description**: Optional

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## Angular CLI

This project was generated with [Angular CLI](https://github.com/angular/angular-cli) version 16.2.16.

### Code Scaffolding
Run `ng generate component component-name` to generate a new component. You can also use `ng generate directive|pipe|service|class|guard|interface|enum|module`.

## Contributing

When making changes:
1. Follow Angular style guide
2. Maintain TypeScript strict mode compliance
3. Add/update tests for new features
4. Ensure responsive design works on mobile
5. Run `npm run build` and `npm test` before committing

## License

This project is part of the Product Catalog Management System.

