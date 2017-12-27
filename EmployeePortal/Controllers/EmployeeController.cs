using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EmployeePortal.Models;
using PagedList;
using PagedList.Mvc;


namespace EmployeePortal.Controllers
{
    public class EmployeeController : Controller
    {
        private EmployeeDBContext db = new EmployeeDBContext();

        //
        // GET: /Employee/

        

        public ActionResult Index(string searchBy, string search, int ? page, string sortBy)
        {
            ViewBag.sortNameParameter = string.IsNullOrEmpty(sortBy) ? "Name Desc" : "";
            ViewBag.sortGenderParameter = sortBy=="Gender" ? "Gender Desc" : "Gender";

            var employees = db.tblEmployees.AsQueryable();

            if (searchBy == "Gender")
            {
                employees = employees.Where(x => x.Gender == search || search == null);
                //var employees = db.Employees.Include(e => e.Department);
                //return View(employees.Where(x => x.Gender == search || search == null).ToList().
                    //ToPagedList(page ?? 1, 3)); // Null coleasce operator to assign page number to 1 if null
            }
            else
            {
                employees = employees.Where(x => x.Name.StartsWith(search) || search == null);
                //var employees = db.Employees.Include(e => e.Department);
                //return View(employees.Where(x => x.Name.StartsWith(search) || search == null).ToList().
                    //ToPagedList(page ?? 1, 3)); 
            }
            switch (sortBy)
            {
                case "Name Desc":
                    employees = employees.OrderByDescending(x => x.Name);
                    break;
                case "Gender Desc":
                    employees = employees.OrderByDescending(x => x.Gender);
                    break;
                case "Gender":
                    employees = employees.OrderBy(x => x.Gender);
                    break;
                default:
                    employees = employees.OrderBy(x => x.Name);
                    break;
            }

            return View(employees.ToPagedList(page ?? 1, 3));  // Implementing Paging
        }

        
        public JsonResult GetEmployees(string term)// Implementing Auto complete feauture for search,Jquery expects "term" as name                                                         
        {
            List<string> employees = db.tblEmployees.Where(x => x.Name.StartsWith(term))
                            .Select(y => y.Name).ToList();
            return Json(employees, JsonRequestBehavior.AllowGet);            
        }                


        public JsonResult IsUserNameAvailable(string Name)   // Validating User name if available
        {
            return Json(!db.tblEmployees.Any(x => x.Name == Name), JsonRequestBehavior.AllowGet);
        }


        //[RequireHttps]        
        public ActionResult EmployeesImages()
        {
            //System.Threading.Thread.Sleep(4000);
            return View(db.tblEmployees.ToList());
        }


        [OutputCache(CacheProfile = "1mincache")]
        public ActionResult EmployeesByDepartment()
        {
            var employees = db.tblEmployees.Include(e => e.tblDepartment)
                .GroupBy(x => x.tblDepartment.DeptName)
                .Select(y => new EmployeesByDepartment
                {
                    DepartmentName = y.Key,
                    EmployeeCount = y.Count()
                }).ToList().OrderByDescending(y => y.EmployeeCount);
            return View(employees.ToList());
        }

        //
        // GET: /Employee/Details/5

        public ActionResult Details(int id = 0)
        {
            tblEmployee employee = db.tblEmployees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        //
        // GET: /Employee/Create

        public ActionResult Create()
        {
            ViewBag.DepartmentId = new SelectList(db.tblDepartments, "ID", "DeptName");
            return View();
        }

        //
        // POST: /Employee/Create

        [HttpPost]
        public ActionResult Create(tblEmployee employee)
        {
            if (ModelState.IsValid)
            {
                db.tblEmployees.Add(employee);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.DepartmentId = new SelectList(db.tblDepartments, "ID", "DeptName", employee.DepartmentId);
            return View(employee);
        }

        //
        // GET: /Employee/Edit/5

        public ActionResult Edit(int id = 0)
        {
            tblEmployee employee = db.tblEmployees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            ViewBag.DepartmentId = new SelectList(db.tblEmployees, "ID", "DeptName", employee.DepartmentId);
            return View(employee);
        }

        //
        // POST: /Employee/Edit/5

        [HttpPost]
        [ActionName("Edit")]
        public ActionResult Edit_Post(int id)
        {
            tblEmployee employee = db.tblEmployees.Single(x => x.EmployeeId == id);
            UpdateModel(employee, null, null, new string[] { "Name" });

            if (ModelState.IsValid)
            {
                db.Entry(employee).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DepartmentId = new SelectList(db.tblDepartments, "ID", "DeptName", employee.DepartmentId);
            return View(employee);
        }

        //
        // GET: /Employee/Delete/5

        public ActionResult Delete(int id = 0)
        {
            tblEmployee employee = db.tblEmployees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        //
        // POST: /Employee/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            tblEmployee employee = db.tblEmployees.Find(id);
            db.tblEmployees.Remove(employee);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //public ActionResult DeleteEmployees(IEnumerable<int> employeeIDs)
        //{
        //    db.Employees.Where(x => employeeIDs.Contains(x.EmployeeId)).ToList()
        //        .ForEach(db.Employees.DeleteObject);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}